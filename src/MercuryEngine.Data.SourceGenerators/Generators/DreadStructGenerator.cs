using System.Text.RegularExpressions;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public class DreadStructGenerator : BaseDreadGenerator<DreadStructType>
{
	// Many fields have a prefix indicating the type; we want to strip that off
	private static readonly Regex FieldNameRegex = new(@"(?:[abcefiopstuv]|v\d|hash|vect|vo|wp|dct|str|arr|dic|map|lst)?([a-zA-Z][a-zA-Z\d_]*)", RegexOptions.Compiled);

	private static readonly List<string> ForbiddenMemberNames = new() { "Write", "Size" };

	public static DreadStructGenerator Instance { get; } = new();

	protected override IEnumerable<string> GenerateSourceLines(DreadStructType dreadType, GenerationContext context)
	{
		var typeName = dreadType.TypeName;
		var typeClassName = TypeNameUtility.SanitizeTypeName(typeName)!;
		var fields = BuildStructFields(dreadType, context);

		yield return $"public class {typeClassName} : DataStructure<{typeClassName}>";
		yield return "{";

		// Emit property declarations
		foreach (var field in fields)
		{
			if (field.HasSummary)
			{
				yield return $"\t/// <summary>";
				yield return $"\t/// {field.GenerateSummary()}";
				yield return $"\t/// </summary>";
			}

			yield return $"\t{field.GenerateProperty()}";
		}

		// Emit data structure description method
		yield return $"\tprotected override void Describe(DataStructureBuilder<{typeClassName}> builder)";
		yield return "\t{";
		yield return "\t\tbuilder.MsePropertyBag(fields => {";

		foreach (var field in fields)
			yield return $"\t\t\t{field.GenerateConfigure()}";

		yield return "\t\t});";
		yield return "\t}";
		yield return "}";

		context.GeneratedTypes.Add(new GeneratedType(typeClassName, typeName));
	}

	private List<IStructField> BuildStructFields(DreadStructType dreadType, GenerationContext context)
	{
		var typeName = dreadType.TypeName;
		var parentTypeName = dreadType.Parent;
		var fields = new List<IStructField>();

		// Add parent fields if necessary
		if (parentTypeName != null)
		{
			if (!context.KnownTypes.TryGetValue(parentTypeName, out var parentBaseType))
				throw new InvalidOperationException($"Struct type \"{typeName}\" has unknown parent type \"{parentTypeName}\"");

			if (parentBaseType is not DreadStructType parentType)
				throw new InvalidOperationException($"Struct type \"{typeName}\" referred to parent type \"{parentTypeName}\", which is not a struct type");

			fields.AddRange(BuildStructFields(parentType, context));
		}

		// Add our own fields
		foreach (var (fieldName, fieldTypeName) in dreadType.Fields)
		{
			if (!context.KnownTypes.TryGetValue(fieldTypeName, out var fieldType))
				throw new InvalidOperationException($"Field \"{fieldName}\" on struct type \"{typeName}\" has unknown type \"{fieldTypeName}\"");

			// Normalize typedefs and pointers to their underlying types
			while (fieldType is DreadPointerType or DreadTypedefType)
			{
				fieldType = fieldType switch {
					DreadPointerType pointerType => FindType(pointerType.Target!, context),
					DreadTypedefType typedefType => FindType(typedefType.Alias!, context),

					_ => throw new InvalidOperationException(),
				};
			}

			try
			{
				var propertyName = GeneratePropertyName(fieldName);
				var propertyType = MapPropertyTypeName(fieldType, context);
				var configureMethod = MapConfigureMethod(fieldType);
				var conflictingFields = fields.OfType<StructField>().Where(f => f.PropertyName == propertyName).ToList();

				if (conflictingFields.Any())
					// Add numeric discriminator
					propertyName += conflictingFields.Count + 1;

				// Fix up forbidden member names
				if (ForbiddenMemberNames.Contains(propertyName))
					propertyName += "_";

				fields.Add(new StructField(fieldName, fieldTypeName, propertyName, propertyType, configureMethod));
			}
			catch (Exception ex)
			{
				fields.Add(new StructFieldError(fieldName, $"{ex.GetType().Name}: {ex.Message.Replace("\r\n", "\n").Replace("\n", "&#10;")}"));
			}
		}

		return fields;
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

			DreadEnumType or DreadFlagsetType or DreadStructType
				=> $"{TypeNameUtility.SanitizeTypeName(dreadType.TypeName)}?",

			DreadVectorType vectorType
				=> $"List<{MapNestedDataTypeName(vectorType.ValueType!, context)}>?",

			DreadDictionaryType dictionaryType
				=> $"Dictionary<{MapNestedDataTypeName(dictionaryType.KeyType!, context)}, {MapNestedDataTypeName(dictionaryType.ValueType!, context)}>?",

			_ => throw new InvalidOperationException($"Unsupported type kind \"{dreadType.Kind}\""),
		};

	private string MapConfigureMethod(BaseDreadType dreadType)
		=> dreadType switch {
			DreadPrimitiveType primitive => primitive.PrimitiveKind switch {
				DreadPrimitiveKind.Float_Vec2 => "Structure",
				DreadPrimitiveKind.Float_Vec3 => "Structure",
				DreadPrimitiveKind.Float_Vec4 => "Structure",

				_ => "Property",
			},

			// Weird case with "char", "double", and "long" defined as structs
			DreadStructType { TypeName: "char" or "double" or "long" }
				=> "Property",

			DreadEnumType or DreadFlagsetType => "Property",
			DreadStructType                   => "Structure",
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
				=> $"EnumDataType<{TypeNameUtility.SanitizeTypeName(enumType.TypeName)}>",

			DreadVectorType vectorType
				=> $"ArrayDataType<{MapNestedDataTypeName(vectorType.ValueType!, context)}>",

			DreadDictionaryType dictionaryType
				=> $"DictionaryDataType<{MapNestedDataTypeName(dictionaryType.KeyType!, context)}, {MapNestedDataTypeName(dictionaryType.ValueType!, context)}>",

			_ => TypeNameUtility.SanitizeTypeName(typeName)!,
		};

	private string MapPrimitiveTypeName(DreadPrimitiveKind primitiveKind)
		=> primitiveKind switch {
			DreadPrimitiveKind.Bool       => "BoolDataType",
			DreadPrimitiveKind.Int        => "Int32DataType",
			DreadPrimitiveKind.UInt       => "UInt32DataType",
			DreadPrimitiveKind.UInt16     => "UInt16DataType",
			DreadPrimitiveKind.UInt64     => "UInt64DataType",
			DreadPrimitiveKind.Float      => "FloatDataType",
			DreadPrimitiveKind.String     => "TerminatedStringDataType",
			DreadPrimitiveKind.Property   => "TerminatedStringDataType",
			DreadPrimitiveKind.Bytes      => "DynamicDreadDataType",
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
		bool   HasSummary { get; }
		string FieldName  { get; }

		string GenerateSummary();
		string GenerateProperty();
		string GenerateConfigure();
	}

	private sealed record StructField(string FieldName, string FieldTypeName, string PropertyName, string PropertyType, string ConfigureMethod) : IStructField
	{
		public bool HasSummary => true;

		public string GenerateSummary()
			=> $"Field: {FieldName}&#10;Original type: {FieldTypeName}";

		public string GenerateProperty()
			=> $"public {PropertyType} {PropertyName} {{ get; set; }}";

		public string GenerateConfigure()
			=> $"fields.{ConfigureMethod}(\"{FieldName}\", m => m.{PropertyName});";
	}

	private sealed record StructFieldError(string FieldName, string Message) : IStructField
	{
		public bool HasSummary => false;

		public string GenerateSummary()
			=> "";

		public string GenerateProperty()
			=> $"// Error generating field \"{FieldName}\": {Message}";

		public string GenerateConfigure()
			=> $"throw new NotSupportedException(\"Cannot read or write field \\\"{FieldName}\\\".\");";
	}

	#endregion
}