using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadStructTypeFactory : BaseDreadDataTypeFactory<DreadStructType, IBinaryDataType>
{
	public static DreadStructTypeFactory Instance { get; } = new();

	protected override IBinaryDataType CreateDataType(DreadStructType dreadType)
	{
		var typeName = dreadType.TypeName;
		var parentTypeName = dreadType.Parent;

		return DynamicStructure.Create(typeName, builder => {
			if (parentTypeName != null)
			{
				if (!DreadTypeRegistry.TryFindType(parentTypeName, out var parentType))
					throw new InvalidOperationException($"Struct type \"{typeName}\" has unknown parent type \"{parentTypeName}\"");

				var parentStructure = DreadTypeRegistry.CreateDataTypeFor(parentType);

				if (parentStructure is not DynamicStructure dynamicParentStructure)
					throw new InvalidOperationException($"Parent type \"{parentTypeName}\" of struct type \"{typeName}\" is not a structure");

				// Add the parent fields to this structure first
				foreach (var field in dynamicParentStructure.Fields)
					builder.AddCopy(field);
			}

			// Add our own fields next
			foreach (var (fieldName, fieldTypeName) in dreadType.Fields)
			{
				if (!DreadTypeRegistry.TryFindType(fieldTypeName, out var fieldType))
					throw new InvalidOperationException($"Field \"{fieldName}\" of struct type \"{typeName}\" has unknown type \"{fieldTypeName}\"");

				builder.AddField(fieldName, () => DreadTypeRegistry.CreateDataTypeFor(fieldType));
			}
		});
	}
}