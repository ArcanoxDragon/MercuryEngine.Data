using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class minimapGrid_TMinimapVisMap : DataStructure<minimapGrid_TMinimapVisMap>,
										  IDescribeDataStructure<minimapGrid_TMinimapVisMap>,
										  ITypedDreadField
{
	public string TypeName => "minimapGrid::TMinimapVisMap";

	public Dictionary<Int32Field, TerminatedStringField> Entries { get; } = [];

	public static void Describe(DataStructureBuilder<minimapGrid_TMinimapVisMap> builder)
		=> builder.Dictionary(m => m.Entries);
}