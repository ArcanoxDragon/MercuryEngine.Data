using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DataTypes;

public class minimapGrid_TMinimapVisMap : DataStructure<minimapGrid_TMinimapVisMap>
{
	public Dictionary<Int32DataType, TerminatedStringDataType> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<minimapGrid_TMinimapVisMap> builder)
		=> builder.Dictionary(m => m.Entries);
}