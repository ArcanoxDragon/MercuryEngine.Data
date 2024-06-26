using System.Text.RegularExpressions;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public class DreadStructGenerator : BaseDreadGenerator<DreadStructType>
{
	// Many fields have a prefix indicating the type; we want to strip that off
	private static readonly Regex        FieldNameRegex       = new(@"(?:[abcefiopstuv]|v\d|hash|vect|vo|wp|dct|str|arr|dic|map|lst)?([a-zA-Z][a-zA-Z\d_]*)", RegexOptions.Compiled);
	private static readonly List<string> ForbiddenMemberNames = ["Write", "Size"];

	public static DreadStructGenerator Instance { get; } = new();

	protected override IEnumerable<string> GenerateSourceLines(DreadStructType dreadType, GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var typeName = dreadType.TypeName;
		var typeClassName = TypeNameUtility.SanitizeTypeName(typeName);
		var preexistingType = generationContext.PreexistingTypes.SingleOrDefault(t => t.Name == typeClassName);
		var fields = BuildStructFields(dreadType, preexistingType, executionContext, generationContext);

		yield return $"public partial class {typeClassName} : DataStructure<{typeClassName}>, IDescribeDataStructure<{typeClassName}>, ITypedDreadField";
		yield return "{";

		// Emit ITypedDreadField implementation
		yield return $"\tpublic string TypeName => \"{typeName}\";";

		// Emit property declarations
		foreach (var field in fields)
		{
			if (!field.ShouldGenerate)
				continue;

			if (field.HasSummary)
			{
				yield return $"\t/// <summary>";
				yield return $"\t/// {field.GenerateSummary()}";
				yield return $"\t/// </summary>";
			}

			yield return $"\t{field.GenerateProperty()}";
		}

		// Emit data structure description method
		yield return $"\tpublic static void Describe(DataStructureBuilder<{typeClassName}> builder)";
		yield return "\t{";
		yield return "\t\tbuilder.MsePropertyBag(fields => {";

		foreach (var field in fields)
			yield return $"\t\t\t{field.GenerateConfigure()}";

		yield return "\t\t});";
		yield return "\t}";
		yield return "}";

		generationContext.GeneratedTypes.Add(new GeneratedType(typeClassName, typeName));
	}

	private List<IStructField> BuildStructFields(DreadStructType dreadType, PreexistingType? preexistingType, GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var typeName = dreadType.TypeName;
		var parentTypeName = dreadType.Parent;
		var fields = new List<IStructField>();

		// Add parent fields if necessary
		if (parentTypeName != null)
		{
			if (!generationContext.KnownTypes.TryGetValue(parentTypeName, out var parentBaseType))
				throw new InvalidOperationException($"Struct type \"{typeName}\" has unknown parent type \"{parentTypeName}\"");

			if (parentBaseType is not DreadStructType parentType)
				throw new InvalidOperationException($"Struct type \"{typeName}\" referred to parent type \"{parentTypeName}\", which is not a struct type");

			fields.AddRange(BuildStructFields(parentType, preexistingType, executionContext, generationContext));
		}

		// Add our own fields
		foreach (var (fieldName, fieldTypeName) in dreadType.Fields)
		{
			if (!generationContext.KnownTypes.TryGetValue(fieldTypeName, out var fieldType))
				throw new InvalidOperationException($"Field \"{fieldName}\" on struct type \"{typeName}\" has unknown type \"{fieldTypeName}\"");

			// Normalize typedefs and pointers to their underlying types
			while (fieldType is DreadPointerType or DreadTypedefType)
			{
				fieldType = fieldType switch {
					DreadPointerType pointerType => FindType(pointerType.Target!, generationContext),
					DreadTypedefType typedefType => FindType(typedefType.Alias!, generationContext),

					_ => throw new InvalidOperationException(),
				};
			}

			try
			{
				var propertyName = GeneratePropertyName(fieldName);
				var propertyType = MapPropertyTypeName(fieldType, generationContext);
				var configureMethod = MapConfigureMethod(fieldType);
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
					if (preexistingProperty.IsReadOnly || preexistingProperty.IsWriteOnly)
					{
						executionContext.ReportDiagnostic(
							Diagnostic.Create(Constants.Diagnostics.PropertyMissingGetterOrSetterDescriptor,
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

				fields.Add(new StructField(fieldName, fieldType.TypeName, propertyName, propertyType, configureMethod, preexistingProperty));
			}
			catch (Exception ex)
			{
				fields.Add(new StructFieldError(fieldName, $"{ex.GetType().Name}: {ex.Message.Replace("\r\n", "\n").Replace("\n", "&#10;")}"));
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
				DreadPrimitiveKind.Bool       => "bool?",
				DreadPrimitiveKind.Int        => "int?",
				DreadPrimitiveKind.UInt       => "uint?",
				DreadPrimitiveKind.UInt16     => "ushort?",
				DreadPrimitiveKind.UInt64     => "ulong?",
				DreadPrimitiveKind.Float      => "float?",
				DreadPrimitiveKind.String     => "string?",
				DreadPrimitiveKind.Property   => "string?",
				DreadPrimitiveKind.Float_Vec2 => "Vector2?",
				DreadPrimitiveKind.Float_Vec3 => "Vector3?",
				DreadPrimitiveKind.Float_Vec4 => "Vector4?",

				_ => throw new InvalidOperationException($"Unsupported primitive kind \"{primitive.PrimitiveKind}\""),
			},

			DreadEnumType or DreadFlagsetType
				=> $"{TypeNameUtility.SanitizeTypeName(dreadType.TypeName)}?",

			DreadStructType
				=> TypeNameUtility.SanitizeTypeName(dreadType.TypeName),

			DreadVectorType vectorType
				=> $"List<{MapNestedDataTypeName(vectorType.ValueType!, context)}>",

			DreadDictionaryType dictionaryType
				=> $"Dictionary<{MapNestedDataTypeName(dictionaryType.KeyType!, context)}, {MapNestedDataTypeName(dictionaryType.ValueType!, context)}>?",

			_ => throw new InvalidOperationException($"Unsupported type kind \"{dreadType.Kind}\""),
		};

	private string MapConfigureMethod(BaseDreadType dreadType)
		=> dreadType switch {
			DreadPrimitiveType primitive => primitive.PrimitiveKind switch {
				DreadPrimitiveKind.Float_Vec2 => "RawProperty",
				DreadPrimitiveKind.Float_Vec3 => "RawProperty",
				DreadPrimitiveKind.Float_Vec4 => "RawProperty",

				_ => "Property",
			},

			// Weird case with "char", "double", and "long" defined as structs
			DreadStructType { TypeName: "char" or "double" or "long" }
				=> "Property",

			DreadEnumType or DreadFlagsetType => "DreadEnum",
			DreadStructType                   => "RawProperty",
			DreadVectorType                   => "Array",
			DreadDictionaryType               => "Dictionary",

			_ => throw new InvalidOperationException($"Unsupported type kind \"{dreadType.Kind}\""),
		};

	private string MapNestedDataTypeName(string typeName, GenerationContext context)
		=> FindType(typeName, context) switch {
			DreadTypedefType typedefType
				=> MapNestedDataTypeName(typedefType.Alias!, context),

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
			DreadPrimitiveKind.Property   => "TerminatedStringField",
			DreadPrimitiveKind.Bytes      => "DreadTypePrefixedField",
			DreadPrimitiveKind.Float_Vec2 => "Vector2",
			DreadPrimitiveKind.Float_Vec3 => "Vector3",
			DreadPrimitiveKind.Float_Vec4 => "Vector4",

			_ => throw new InvalidOperationException($"Unsupported primitive kind \"{primitiveKind}\""),
		};

	private BaseDreadType FindType(string typeName, GenerationContext context)
	{
		if (!context.KnownTypes.TryGetValue(typeName, out var knownType))
			throw new InvalidOperationException($"Unknown type \"{typeName}\"");

		return knownType;
	}

	#region Helper Types

	private interface IStructField
	{
		bool   ShouldGenerate { get; }
		bool   HasSummary     { get; }
		string FieldName      { get; }

		string GenerateSummary();
		string GenerateProperty();
		string GenerateConfigure();
	}

	private sealed record StructField(string FieldName, string FieldTypeName, string PropertyName, string PropertyType, string ConfigureMethod, IPropertySymbol? PreexistingProperty) : IStructField
	{
		public bool ShouldGenerate => PreexistingProperty is null;
		public bool HasSummary     => true;

		public string GenerateSummary()
			=> $"Field: {FieldName}&#10;Original type: {TypeNameUtility.XmlEscapeTypeName(FieldTypeName)}";

		public string GenerateProperty()
			=> $"public {PropertyType} {PropertyName} {{ get; set; }}";

		public string GenerateConfigure()
			=> $"fields.{ConfigureMethod}(\"{FieldName}\", m => m.{PropertyName});";
	}

	private sealed record StructFieldError(string FieldName, string Message) : IStructField
	{
		public bool ShouldGenerate => true;
		public bool HasSummary     => false;

		public string GenerateSummary()
			=> "";

		public string GenerateProperty()
			=> $"// Error generating field \"{FieldName}\": {Message}";

		public string GenerateConfigure()
			=> $"throw new NotSupportedException(\"Cannot read or write field \\\"{FieldName}\\\".\");";
	}

	#endregion
}