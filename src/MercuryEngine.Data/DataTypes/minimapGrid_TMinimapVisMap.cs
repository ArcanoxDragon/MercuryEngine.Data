using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class minimapGrid_TMinimapVisMap : DataStructure<minimapGrid_TMinimapVisMap>
{
	public Dictionary<Int32DataType, TerminatedStringDataType> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<minimapGrid_TMinimapVisMap> builder)
		=> builder.Dictionary(m => m.Entries);
}