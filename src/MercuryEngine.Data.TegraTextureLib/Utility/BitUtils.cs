using System.Numerics;

namespace MercuryEngine.Data.TegraTextureLib.Utility;

internal static class BitUtils
{
	public static T AlignUp<T>(T value, T size)
	where T : IBinaryInteger<T>
		=> ( value + ( size - T.One ) ) & -size;

	public static T DivRoundUp<T>(T numerator, T denominator)
	where T : IBinaryInteger<T>
		=> ( numerator + denominator - T.One ) / denominator;
}