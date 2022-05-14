using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DataTypes;

public class TEnabledOccluderCollidersMap : DataStructure<TEnabledOccluderCollidersMap>
{
	public List<Entry> Items { get; } = new();

	protected override void Describe(DataStructureBuilder<TEnabledOccluderCollidersMap> builder)
		=> builder.Array(m => m.Items);

	public sealed class Entry : DataStructure<Entry>
	{
		public string Key { get; set; } = string.Empty;
		public List<UInt64DataType> EnabledIds { get; } = new();

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Property(m => m.Key)
					  .Array(m => m.EnabledIds);
	}
}