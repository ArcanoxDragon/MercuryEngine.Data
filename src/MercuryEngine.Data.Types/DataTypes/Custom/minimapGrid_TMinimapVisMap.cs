using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DataTypes.Custom;

public class minimapGrid_TMinimapVisMap : DataStructure<minimapGrid_TMinimapVisMap>, IDreadDataType
{
	public string TypeName => "minimapGrid::TMinimapVisMap";

	public Dictionary<Int32DataType, TerminatedStringDataType> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<minimapGrid_TMinimapVisMap> builder)
		=> builder.Dictionary(m => m.Entries);
}