using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class TEnabledOccluderCollidersMap : DataStructure<TEnabledOccluderCollidersMap>,
											IDescribeDataStructure<TEnabledOccluderCollidersMap>,
											ITypedDreadField
{
	public string TypeName => "TEnabledOccluderCollidersMap";

	public List<Entry> Items { get; } = [];

	public static void Describe(DataStructureBuilder<TEnabledOccluderCollidersMap> builder)
		=> builder.Array(m => m.Items);

	public sealed class Entry : DataStructure<Entry>, IDescribeDataStructure<Entry>
	{
		public string            Key        { get; set; } = string.Empty;
		public List<UInt64Field> EnabledIds { get; }      = [];

		public static void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Property(m => m.Key)
				.Array(m => m.EnabledIds);
	}
}