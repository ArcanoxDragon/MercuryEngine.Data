namespace MercuryEngine.Data.Converters.Utility;

internal static class ColorUtility
{
	public static (float H, float S, float L) RgbToHsl(float r, float g, float b)
	{
		var maxComp = Math.Max(r, Math.Max(g, b));
		var minComp = Math.Min(r, Math.Min(g, b));
		var chroma = maxComp - minComp;
		float hueSector;

		// ReSharper disable CompareOfFloatsByEqualityOperator
		if (Math.Abs(chroma) < 1e-7)
			// Undefined (no chroma) - default to red
			hueSector = 0f;
		else if (maxComp == r)
			hueSector = ( g - b ) / chroma % 6f;
		else if (maxComp == g)
			hueSector = ( ( b - r ) / chroma ) + 2f;
		else // maxComp == b
			hueSector = ( ( r - g ) / chroma ) + 4f;
		// ReSharper restore CompareOfFloatsByEqualityOperator

		var hue = hueSector * 60f;
		var lightness = 0.5f * ( maxComp - minComp );
		float saturation;

		if (lightness is <= 0f or >= 1f)
			saturation = 0f;
		else
			saturation = chroma / ( 1 - Math.Abs(( 2 * lightness ) - 1) );

		// Clamp for safety
		hue = Math.Clamp(hue, 0f, 360f);
		saturation = Math.Clamp(saturation, 0f, 1f);
		lightness = Math.Clamp(lightness, 0f, 1f);

		return ( hue, saturation, lightness );
	}
}