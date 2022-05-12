using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadStructType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Struct;

	public string?                    Parent { get; set; }
	public Dictionary<string, string> Fields { get; set; } = new();

	public override IBinaryDataType CreateDataType()
		=> DynamicStructure.Create(TypeName, builder => {
			if (Parent != null)
			{
				if (!DreadTypeRegistry.TryFindType(Parent, out var parentType))
					throw new InvalidOperationException($"Struct type \"{TypeName}\" has unknown parent type \"{Parent}\"");

				var parentStructure = parentType.CreateDataType();

				if (parentStructure is not DynamicStructure dynamicParentStructure)
					throw new InvalidOperationException($"Parent type \"{Parent}\" of struct type \"{TypeName}\" is not a structure");

				// Add the parent fields to this structure first
				foreach (var field in dynamicParentStructure.Fields)
					builder.AddCopy(field);
			}

			// Add our own fields next
			foreach (var (fieldName, fieldTypeName) in Fields)
			{
				if (!DreadTypeRegistry.TryFindType(fieldTypeName, out var fieldType))
					throw new InvalidOperationException($"Field \"{fieldName}\" of struct type \"{TypeName}\" has unknown type \"{fieldTypeName}\"");

				builder.AddField(fieldName, () => fieldType.CreateDataType());
			}
		});
}