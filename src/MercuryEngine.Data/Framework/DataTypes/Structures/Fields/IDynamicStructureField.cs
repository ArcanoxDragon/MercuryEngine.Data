namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

public interface IDynamicStructureField : IDataStructureField
{
	string          FieldName { get; }
	IBinaryDataType Data      { get; }
	dynamic         Value     { get; set; }
}