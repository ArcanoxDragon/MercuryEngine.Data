using JetBrains.Annotations;

namespace MercuryEngine.Data.DataTypes;

[PublicAPI]
public enum EBreakableTileType
{
	UNDEFINED     = 0,
	POWERBEAM     = 1,
	BOMB          = 2,
	MISSILE       = 3,
	SUPERMISSILE  = 4,
	POWERBOMB     = 5,
	SCREWATTACK   = 6,
	WEIGHT        = 7,
	BABYHATCHLING = 8,
	SPEEDBOOST    = 9,
}