using System.Numerics;

namespace MercuryEngine.Data.Converters.Extensions;

internal static class VectorExtensions
{
	public static bool IsSingleComponentSet(this Vector4 vector)
	{
		if (Math.Abs(vector.X) > 0 && vector is { Y: 0, Z: 0, W: 0 })
			return true;
		if (Math.Abs(vector.Y) > 0 && vector is { X: 0, Z: 0, W: 0 })
			return true;
		if (Math.Abs(vector.Z) > 0 && vector is { X: 0, Y: 0, W: 0 })
			return true;
		if (Math.Abs(vector.W) > 0 && vector is { X: 0, Y: 0, Z: 0 })
			return true;

		return false;
	}
}