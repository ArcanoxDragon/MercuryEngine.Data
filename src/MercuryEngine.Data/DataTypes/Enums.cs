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

[PublicAPI]
public enum EMarkerType
{
	MARKER_A = 0,
	MARKER_B = 1,
	MARKER_C = 2,
	MARKER_D = 3,
	MARKER_E = 4,
	MARKER_F = 5,
	MARKER_G = 6,
	MARKER_H = 7,
	MARKER_I = 8,
}