using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class ActorTileState : DataStructure<ActorTileState>
{
	public float              X        { get; set; }
	public float              Y        { get; set; }
	public EBreakableTileType TileType { get; set; }
	public uint               State    { get; set; }

	protected override void Describe(DataStructureBuilder<ActorTileState> builder)
		=> builder.Int32(4)
				  .CrcLiteral("fX")
				  .Float(m => m.X)
				  .CrcLiteral("fY")
				  .Float(m => m.Y)
				  .CrcLiteral("eTileType")
				  .Enum(m => m.TileType)
				  .CrcLiteral("uState")
				  .UInt32(m => m.State);
}