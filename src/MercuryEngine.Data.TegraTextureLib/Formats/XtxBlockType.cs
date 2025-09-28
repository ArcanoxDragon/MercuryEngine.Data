namespace MercuryEngine.Data.TegraTextureLib.Formats;

public enum XtxBlockType : uint
{
	Unknown,
	TextureInfo = 2,
	TextureData = 3,
	EndOfFile   = 5,
}