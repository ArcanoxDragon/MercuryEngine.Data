namespace MercuryEngine.Data.Types.Bsmat;

public enum TranslucencyType
{
	None          = 0,
	Opaque        = 1,
	Translucent   = 2,
	Subtractive   = 4,
	Additive      = 8,
	OpaqueForward = 16,
	All           = 31,
	NotOpaque     = 14,
}