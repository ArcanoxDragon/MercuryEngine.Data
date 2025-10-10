using System.Diagnostics;
using System.Numerics;

namespace MercuryEngine.Data.Converters.Utility;

public class CubicHermiteSpline
{
	private const float Tolerance = 1e-5f;

	private readonly SplinePoint[] points;
	private readonly float[]       times;

	public CubicHermiteSpline(params IEnumerable<SplinePoint> points)
		: this(points.ToArray()) { }

	public CubicHermiteSpline(params SplinePoint[] points)
	{
		this.points = points;
		this.times = new float[points.Length];

		var minTime = float.PositiveInfinity;
		var maxTime = float.NegativeInfinity;
		var prevTime = float.NegativeInfinity;

		for (var i = 0; i < points.Length; i++)
		{
			var time = points[i].Time;

			minTime = float.Min(minTime, time);
			maxTime = float.Max(maxTime, time);

			if (time < prevTime)
				throw new ArgumentException($"Points must be specified in ascending time order. " +
											$"Point {i} had a smaller time value than point {i - 1}.",
											nameof(points));

			this.times[i] = time;
			prevTime = time;
		}

		MinimumTime = minTime;
		MaximumTime = maxTime;
	}

	public IReadOnlyList<SplinePoint> Points => this.points;

	public IReadOnlyList<float> PointTimes => this.times;

	public float MinimumTime { get; }
	public float MaximumTime { get; }

	public float GetValueAt(float time)
	{
		// Edge cases first
		if (Math.Abs(time - MinimumTime) <= Tolerance)
			return this.points[0].Value;
		if (Math.Abs(time - MaximumTime) <= Tolerance)
			return this.points[^1].Value;

		var k = GetTimeIndex(time);
		var time0 = this.times[k];
		var time1 = this.times[k + 1];
		var range = time1 - time0;
		var t = ( time - time0 ) / range;

		var (_, v0, m0) = this.points[k];
		var (_, v1, m1) = this.points[k + 1];

		var basisVector = GetValueBasisVector(t);
		var valuesVector = new Vector4(v0, m0, v1, m1);
		var rangeVector = new Vector4(1, range, 1, range);

		return Vector4.Dot(basisVector, valuesVector * rangeVector);
	}

	public float GetDerivativeAt(float time)
	{
		// Edge cases first
		if (Math.Abs(time - MinimumTime) <= Tolerance)
			return this.points[0].Derivative;
		if (Math.Abs(time - MaximumTime) <= Tolerance)
			return this.points[^1].Derivative;

		var k = GetTimeIndex(time);
		var time0 = this.times[k];
		var time1 = this.times[k + 1];
		var range = time1 - time0;
		var t = ( time - time0 ) / range;

		var (_, v0, m0) = this.points[k];
		var (_, v1, m1) = this.points[k + 1];

		var basisVector = GetDerivativeBasisVector(t);
		var valuesVector = new Vector4(v0, m0, v1, m1);
		var rangeVector = new Vector4(1, range, 1, range);

		return Vector4.Dot(basisVector, valuesVector * rangeVector);
	}

	private int GetTimeIndex(float time)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(time, MinimumTime);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(time, MaximumTime);

		// At this point we know that "time" MUST BE within our domain

		// If this returns a non-negative number, "time" is on one of our control points.
		// If it returns a negative number, the bitwise complement of that number is the
		// index of the control point AFTER "time", so we need to take (bitwise complement
		// - 1) to get the control point BEFORE "time".
		var k = Array.BinarySearch(this.times, time);

		if (k < 0)
			k = ~k - 1;

		Debug.Assert(k >= 0, "Time out-of-range");
		Debug.Assert(k < this.points.Length - 1, "Time out-of-range");

		return k;
	}

	#region Basis Helpers

	private static readonly Matrix4x4 ValueBasisMatrix = new(
		2, 1, -2, 1,
		-3, -2, 3, -1,
		0, 1, 0, 0,
		1, 0, 0, 0
	);

	private static readonly Matrix4x4 DerivativeBasisMatrix = new(
		0, 0, 0, 0,
		6, 3, -6, 3,
		-6, 0, 6, -2,
		0, -2, 0, 0
	);

	private static Vector4 GetValueBasisVector(float t)
	{
		var t2 = t * t;
		var t3 = t2 * t;

		return Vector4.Transform(new Vector4(t3, t2, t, 1f), ValueBasisMatrix);
	}

	private static Vector4 GetDerivativeBasisVector(float t)
	{
		var t2 = t * t;
		const float t3 = 0f; // We cheat because the first column of the matrix is all 0s - we don't need to compute t3 at all

		return Vector4.Transform(new Vector4(t3, t2, t, 1f), DerivativeBasisMatrix);
	}

	#endregion
}