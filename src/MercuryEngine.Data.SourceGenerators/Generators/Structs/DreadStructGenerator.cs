using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators.Structs;

public class DreadStructGenerator : BaseDreadGenerator<DreadStructType>
{
	// Many fields have a prefix indicating the type; we want to strip that off
	private static readonly Regex        FieldNameRegex       = new(@"(?:v\d|hash|vect|vo|wp|dct|str|arr|dic|map|lst|[abcefinoprstuv])?([a-zA-Z][a-zA-Z\d_]*)", RegexOptions.Compiled);
	private static readonly List<string> ForbiddenMemberNames = ["Write", "Size", "Reset"];

	public static DreadStructGenerator Instance { get; } = new();

	protected override IEnumerable<string> GenerateSourceLines(DreadStructType dreadType, GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var typeName = dreadType.TypeName;
		var parentTypeName = dreadType.Parent;

		var typeClassName = TypeNameUtility.SanitizeTypeName(typeName);
		var parentClassName = default(string);

		var preexistingType = generationContext.PreexistingTypes.SingleOrDefault(t => t.Name == typeClassName);
		var fields = BuildStructFields(dreadType, preexistingType, executionContext, generationContext);

		if (parentTypeName != null)
		{
			parentClassName = TypeNameUtility.SanitizeTypeName(parentTypeName);

			yield return $"public partial class {typeClassName} : {parentClassName}";
			yield return "{";

			// Emit ITypedDreadField implementation
			yield return $"\tpublic override string TypeName => \"{typeName}\";";
		}
		else
		{
			yield return $"public partial class {typeClassName} : BaseDreadDataStructure<{typeClassName}>, ITypedDreadField";
			yield return "{";

			// Emit ITypedDreadField implementation
			yield return $"\tpublic virtual string TypeName => \"{typeName}\";";
		}

		// Emit property declarations
		foreach (var field in fields.Where(f => f.ShouldGenerate))
		{
			if (field.HasSummary)
			{
				yield return "\t/// <summary>";
				yield return $"\t/// {field.GenerateSummary()}";
				yield return "\t/// </summary>";
			}

			foreach (var line in field.GenerateProperty())
				yield return $"\t{line}";
		}

		// Emit field configuration method
		yield return "";
		yield return "\tprotected override void DefineFields(PropertyBagFieldBuilder fields)";
		yield return "\t{";

		if (parentClassName != null)
			yield return "\t\tbase.DefineFields(fields);";

		foreach (var field in fields)
			yield return $"\t\t{field.GenerateDefine()}";

		yield return "\t}";
		yield return "}";

		generationContext.GeneratedTypes.Add(new GeneratedType(typeClassName, typeName, parentTypeName));
	}

	private List<IStructField> BuildStructFields(DreadStructType dreadType, PreexistingType? preexistingType, GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var typeName = dreadType.TypeName;
		var fields = new List<IStructField>();

		// Add our own fields
		foreach (var (fieldName, fieldTypeName) in dreadType.Fields)
		{
			if (!generationContext.KnownTypes.TryGetValue(fieldTypeName, out var fieldType))
				throw new InvalidOperationException($"Field \"{fieldName}\" on struct type \"{typeName}\" has unknown type \"{fieldTypeName}\"");

			if (FieldExistsOnParent(dreadType, fieldName, generationContext, out var parentWithDuplicate))
			{
				fields.Add(new SkippedStructField(fieldName, $"the field was already defined on the parent type \"{parentWithDuplicate.TypeName}\""));
				continue;
			}

			// Normalize typedefs to their underlying type
			while (fieldType is DreadTypedefType)
			{
				fieldType = fieldType switch {
					DreadTypedefType typedefType => FindType(typedefType.Alias!, generationContext),

					_ => throw new InvalidOperationException(),
				};
			}

			try
			{
				var propertyName = GeneratePropertyName(fieldName);
				var propertyType = MapPropertyTypeName(fieldType, generationContext);
				var propertyTypeGenericArgs = MapPropertyTypeGenericArgs(fieldType, generationContext);
				var configureMethod = MapConfigureMethod(fieldType, generationContext);
				var structFieldKind = MapStructFieldKind(fieldType, generationContext);
				var preexistingProperty = FindMatchingProperty(preexistingType, fieldName);

				if (preexistingProperty != null)
				{
					// A pre-existing type was defined (i.e. a partial class for this generated class) and there is a property
					// that is marked as the backing property for this struct field. We must ensure that the property type matches
					// what we need it to be, and as long as it does, we will use that property instead of defining a new one.

					var requiredPropertyTypeName = propertyType.Trim('?');
					var preexistingPropertyTypeName = preexistingProperty.Type.GetSimpleGenericName().Trim('?');

					if (preexistingPropertyTypeName != requiredPropertyTypeName)
					{
						executionContext.ReportDiagnostic(
							Diagnostic.Create(Constants.Diagnostics.PropertyTypeMismatchDescriptor,
											  preexistingProperty.Locations.First(),
											  preexistingProperty.Locations.Skip(1),
											  preexistingProperty.Name,
											  $"{preexistingType!.Namespace}.{preexistingType.Name}",
											  requiredPropertyTypeName,
											  fieldName));

						continue;
					}

					// If a pre-existing property exists, it must have both a getter AND a setter.
					var canBeReadOnly = structFieldKind is not StructFieldKind.BasicValue;

					if (preexistingProperty.IsWriteOnly || ( preexistingProperty.IsReadOnly && !canBeReadOnly ))
					{
						var diagnosticDescriptor = canBeReadOnly
							? Constants.Diagnostics.PropertyMissingGetterDescriptor
							: Constants.Diagnostics.PropertyMissingGetterOrSetterDescriptor;

						executionContext.ReportDiagnostic(
							Diagnostic.Create(diagnosticDescriptor,
											  preexistingProperty.Locations.First(),
											  preexistingProperty.Locations.Skip(1),
											  preexistingProperty.Name,
											  $"{preexistingType!.Namespace}.{preexistingType.Name}",
											  fieldName));

						continue;
					}

					propertyName = preexistingProperty.Name;
				}

				var conflictingFields = fields.OfType<StructField>().Where(f => f.PropertyName == propertyName).ToList();

				if (conflictingFields.Any())
					// Add numeric discriminator
					propertyName += conflictingFields.Count + 1;

				// Fix up forbidden member names
				if (ForbiddenMemberNames.Contains(propertyName))
					propertyName += "_";

				fields.Add(new StructField(
							   fieldName, fieldType.TypeName, propertyName,
							   propertyType, propertyTypeGenericArgs,
							   configureMethod, structFieldKind, preexistingProperty));
			}
			catch (Exception ex)
			{
				fields.Add(new InvalidStructField(fieldName, $"{ex.GetType().Name}: {ex.Message.Replace("\r\n", "\n").Replace("\n", "&#10;")}"));
			}
		}

		return fields;
	}

	private IPropertySymbol? FindMatchingProperty(PreexistingType? preexistingType, string fieldName)
	{
		if (preexistingType is null)
			return null;

		foreach (var property in preexistingType.Properties)
		{
			var attributes = property.GetAttributes();

			foreach (var attribute in attributes)
			{
				if (attribute.AttributeClass?.GetFullyQualifiedName() != Constants.StructPropertyAttributeClassName)
					continue;

				var fieldNameArgument = attribute.ConstructorArguments.FirstOrDefault();

				if (fieldNameArgument is not { Type.SpecialType: SpecialType.System_String, Value: string propertyFieldName })
					continue;

				if (propertyFieldName == fieldName)
					return property;
			}
		}

		return null;
	}

	private string GeneratePropertyName(string fieldName)
	{
		if (FieldNameRegex.Match(fieldName) is not { Success: true } match)
			throw new ArgumentException($"Invalid field name: {fieldName}", nameof(fieldName));

		return match.Groups[1].Value;
	}

	private string MapPropertyTypeName(BaseDreadType dreadType, GenerationContext context)
		=> dreadType switch {
			DreadPrimitiveType primitive => primitive.PrimitiveKind switch {
				DreadPrimitiveKind.Bool       => "bool",
				DreadPrimitiveKind.Int        => "int",
				DreadPrimitiveKind.UInt       => "uint",
				DreadPrimitiveKind.UInt16     => "ushort",
				DreadPrimitiveKind.UInt64     => "ulong",
				DreadPrimitiveKind.Float      => "float",
				DreadPrimitiveKind.String     => "string",
				DreadPrimitiveKind.Property   => "StrId",
				DreadPrimitiveKind.Float_Vec2 => "Vector2",
				DreadPrimitiveKind.Float_Vec3 => "Vector3",
				DreadPrimitiveKind.Float_Vec4 => "Vector4",

				_ => throw new InvalidOperationException($"Unsupported primitive kind \"{primitive.PrimitiveKind}\""),
			},

			DreadPointerType pointerType when FindType(pointerType.Target!, context) is DreadStructType
				=> $"DreadPointer<{MapNestedDataTypeName(pointerType.Target!, context)}>",

			DreadPointerType pointerType
				=> MapPropertyTypeName(FindType(pointerType.Target!, context), context),

			DreadEnumType or DreadFlagsetType or DreadStructType
				=> TypeNameUtility.SanitizeTypeName(dreadType.TypeName),

			DreadVectorType vectorType
				=> $"IList<{MapNestedDataTypeName(vectorType.ValueType!, context)}>",

			DreadDictionaryType dictionaryType
				=> $"IDictionary<{MapNestedDataTypeName(dictionaryType.KeyType!, context)}, {MapNestedDataTypeName(dictionaryType.ValueType!, context)}>",

			_ => throw new InvalidOperationException($"Unsupported type kind \"{dreadType.Kind}\""),
		};

	private string[] MapPropertyTypeGenericArgs(BaseDreadType dreadType, GenerationContext context)
		=> dreadType switch {
			DreadPointerType pointerType when FindType(pointerType.Target!, context) is DreadStructType
				=> [MapNestedDataTypeName(pointerType.Target!, context)],

			DreadPointerType pointerType
				=> MapPropertyTypeGenericArgs(FindType(pointerType.Target!, context), context),

			DreadVectorType vectorType
				=> [MapNestedDataTypeName(vectorType.ValueType!, context)],

			DreadDictionaryType dictionaryType
				=> [MapNestedDataTypeName(dictionaryType.KeyType!, context), MapNestedDataTypeName(dictionaryType.ValueType!, context)],

			_ => [],
		};

	private string MapConfigureMethod(BaseDreadType dreadType, GenerationContext context)
		=> dreadType switch {
			DreadPrimitiveType primitive => primitive.PrimitiveKind switch {
				DreadPrimitiveKind.Bool   => "Boolean",
				DreadPrimitiveKind.Float  => "Float",
				DreadPrimitiveKind.Int    => "Int32",
				DreadPrimitiveKind.UInt   => "UInt32",
				DreadPrimitiveKind.UInt16 => "UInt16",
				DreadPrimitiveKind.UInt64 => "UInt64",
				DreadPrimitiveKind.String => "String",

				DreadPrimitiveKind.Property   => "AddField",
				DreadPrimitiveKind.Float_Vec2 => "AddField",
				DreadPrimitiveKind.Float_Vec3 => "AddField",
				DreadPrimitiveKind.Float_Vec4 => "AddField",

				_ => "Property",
			},

			// Weird case with "char", "double", and "long" defined as structs
			DreadStructType { TypeName: "char" }   => "Char",
			DreadStructType { TypeName: "double" } => "Double",
			DreadStructType { TypeName: "long" }   => "Int64",
			DreadStructType                        => "AddField",

			DreadPointerType pointerType when FindType(pointerType.Target!, context) is DreadStructType
				=> "AddField",

			DreadPointerType pointerType
				=> MapConfigureMethod(FindType(pointerType.Target!, context), context),

			DreadEnumType or DreadFlagsetType
				=> $"DreadEnum<{MapPropertyTypeName(dreadType, context)}>",

			DreadVectorType vectorType
				=> $"Array<{MapNestedDataTypeName(vectorType.ValueType!, context)}>",

			DreadDictionaryType dictionaryType
				=> $"Dictionary<{MapNestedDataTypeName(dictionaryType.KeyType!, context)}, {MapNestedDataTypeName(dictionaryType.ValueType!, context)}>",

			_ => throw new InvalidOperationException($"Unsupported type kind \"{dreadType.Kind}\""),
		};

	private StructFieldKind MapStructFieldKind(BaseDreadType dreadType, GenerationContext context)
		=> dreadType switch {
			DreadPrimitiveType primitive => primitive.PrimitiveKind switch {
				DreadPrimitiveKind.Property   => StructFieldKind.RawField,
				DreadPrimitiveKind.Float_Vec2 => StructFieldKind.RawField,
				DreadPrimitiveKind.Float_Vec3 => StructFieldKind.RawField,
				DreadPrimitiveKind.Float_Vec4 => StructFieldKind.RawField,

				_ => StructFieldKind.BasicValue,
			},

			// Weird case with "char", "double", and "long" defined as structs
			DreadStructType { TypeName: "char" or "double" or "long" }
				=> StructFieldKind.BasicValue,

			DreadPointerType pointerType when FindType(pointerType.Target!, context) is DreadStructType
				=> StructFieldKind.RawField,

			DreadPointerType pointerType
				=> MapStructFieldKind(FindType(pointerType.Target!, context), context),

			DreadEnumType or DreadFlagsetType => StructFieldKind.BasicValue,
			DreadStructType                   => StructFieldKind.RawField,
			DreadVectorType                   => StructFieldKind.Array,
			DreadDictionaryType               => StructFieldKind.Dictionary,

			_ => throw new InvalidOperationException($"Unsupported type kind \"{dreadType.Kind}\""),
		};

	private string MapNestedDataTypeName(string typeName, GenerationContext context)
		=> FindType(typeName, context) switch {
			DreadTypedefType typedefType
				=> MapNestedDataTypeName(typedefType.Alias!, context),

			// This is a special case to avoid a redundant type-prefixed pointer to a type-prefixed value
			/* DreadPointerType pointerType when FindType(pointerType.Target!, context)
					is DreadPrimitiveType { PrimitiveKind: DreadPrimitiveKind.Bytes }
				=> MapNestedDataTypeName(pointerType.Target!, context), */

			DreadPointerType pointerType when FindType(pointerType.Target!, context) is DreadStructType
				=> $"DreadPointer<{MapNestedDataTypeName(pointerType.Target!, context)}>",

			DreadPointerType pointerType
				=> MapNestedDataTypeName(pointerType.Target!, context),

			DreadPrimitiveType primitiveType
				=> MapPrimitiveTypeName(primitiveType.PrimitiveKind),

			DreadEnumType enumType
				=> $"DreadEnum<{TypeNameUtility.SanitizeTypeName(enumType.TypeName)}>",

			DreadFlagsetType flagsetType
				=> $"DreadEnum<{TypeNameUtility.SanitizeTypeName(flagsetType.Enum)}>",

			DreadVectorType vectorType
				=> $"ArrayField<{MapNestedDataTypeName(vectorType.ValueType!, context)}>",

			DreadDictionaryType dictionaryType
				=> $"DictionaryField<{MapNestedDataTypeName(dictionaryType.KeyType!, context)}, {MapNestedDataTypeName(dictionaryType.ValueType!, context)}>",

			_ => TypeNameUtility.SanitizeTypeName(typeName),
		};

	private string MapPrimitiveTypeName(DreadPrimitiveKind primitiveKind)
		=> primitiveKind switch {
			DreadPrimitiveKind.Bool       => "BooleanField",
			DreadPrimitiveKind.Int        => "Int32Field",
			DreadPrimitiveKind.UInt       => "UInt32Field",
			DreadPrimitiveKind.UInt16     => "UInt16Field",
			DreadPrimitiveKind.UInt64     => "UInt64Field",
			DreadPrimitiveKind.Float      => "FloatField",
			DreadPrimitiveKind.String     => "TerminatedStringField",
			DreadPrimitiveKind.Property   => "StrId",
			DreadPrimitiveKind.Bytes      => "DreadTypePrefixedField",
			DreadPrimitiveKind.Float_Vec2 => "Vector2",
			DreadPrimitiveKind.Float_Vec3 => "Vector3",
			DreadPrimitiveKind.Float_Vec4 => "Vector4",

			_ => throw new InvalidOperationException($"Unsupported primitive kind \"{primitiveKind}\""),
		};

	private static bool FieldExistsOnParent(DreadStructType structType, string fieldName, GenerationContext context, [NotNullWhen(true)] out DreadStructType? containingStruct)
	{
		containingStruct = default;

		if (structType.Parent is null)
			return false;

		if (FindType(structType.Parent, context) is not DreadStructType parentStruct)
			return false;

		if (parentStruct.Fields.ContainsKey(fieldName))
		{
			containingStruct = parentStruct;
			return true;
		}

		return FieldExistsOnParent(parentStruct, fieldName, context, out containingStruct);
	}

	private static BaseDreadType FindType(string typeName, GenerationContext context)
	{
		if (!context.KnownTypes.TryGetValue(typeName, out var knownType))
			throw new InvalidOperationException($"Unknown type \"{typeName}\"");

		return knownType;
	}
}