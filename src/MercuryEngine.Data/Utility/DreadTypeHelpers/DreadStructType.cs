using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadStructType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Struct;

	public string?                    Parent { get; set; }
	public Dictionary<string, string> Fields { get; set; } = new();

	public override IBinaryDataType CreateDataType()
		=> DynamicStructure.Create(TypeName, builder => {
			if (Parent != null)
			{
				if (DreadTypes.FindType(Parent) is not { } parentType)
					throw new InvalidOperationException($"Struct type \"{TypeName}\" has unknown parent type \"{Parent}\"");

				var parentStructure = parentType.CreateDataType();

				if (parentStructure is not DynamicStructure dynamicParentStructure)
					throw new InvalidOperationException($"Parent type \"{Parent}\" of struct type \"{TypeName}\" is not a structure");

				// Add the parent fields to this structure first
				foreach (var field in dynamicParentStructure.Fields)
					builder.AddField(field.FieldName, field.Data);
			}

			// Add our own fields next
			foreach (var (fieldName, fieldTypeName) in Fields)
			{
				if (DreadTypes.FindType(fieldTypeName) is not { } fieldType)
					throw new InvalidOperationException($"Field \"{fieldName}\" of struct type \"{TypeName}\" has unknown type \"{fieldTypeName}\"");

				var fieldDataType = fieldType.CreateDataType();

				builder.AddField(fieldName, fieldDataType);
			}
		});
}