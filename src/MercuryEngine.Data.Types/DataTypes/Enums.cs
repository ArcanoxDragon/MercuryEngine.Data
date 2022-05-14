using JetBrains.Annotations;

namespace MercuryEngine.Data.Types.DataTypes;

[PublicAPI]
public enum EBreakableTileType
{
	UNDEFINED = 0,
	POWERBEAM = 1,
	BOMB = 2,
	MISSILE = 3,
	SUPERMISSILE = 4,
	POWERBOMB = 5,
	SCREWATTACK = 6,
	WEIGHT = 7,
	BABYHATCHLING = 8,
	SPEEDBOOST = 9,
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

[PublicAPI]
public enum EEntryType
{
	ML_INFO_ENTRY = 0,
	ML_DIALOGUE_ENTRY = 1,
	ML_TUTO_ENTRY = 2,
}

[PublicAPI]
public enum EMapTutoType
{
	HINT_ZONE = 0,
	EMMY_ZONE = 1,
	TELEPORTER_NET = 2,
}