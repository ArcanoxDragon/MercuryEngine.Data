using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using SysVector4 = System.Numerics.Vector4;

namespace MercuryEngine.Data.Types.DreadTypes;

[DebuggerDisplay("<{X}, {Y}, {Z}, {W}>")]
public class Vector4b : DataStructure<Vector4b>
{
	public Vector4b() { }

	public Vector4b(byte x, byte y, byte z, byte w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public byte X { get; set; }
	public byte Y { get; set; }
	public byte Z { get; set; }
	public byte W { get; set; }

	/// <summary>
	/// Converts this <see cref="Vector4b"/> to a <see cref="System.Numerics.Vector4"/> value.
	/// </summary>
	/// <param name="scale">
	/// If <see langword="true"/>, the byte values of the components will be scaled to float
	/// values between 0 and 1 during conversion. If <see langword="false"/>, the values will
	/// simply be cast to a float.
	/// </param>
	public SysVector4 ToSystemVector(bool scale = false)
	{
		var x = scale ? ( X / 255f ) : X;
		var y = scale ? ( Y / 255f ) : Y;
		var z = scale ? ( Z / 255f ) : Z;
		var w = scale ? ( W / 255f ) : W;

		return new SysVector4(x, y, z, w);
	}

	protected override void Describe(DataStructureBuilder<Vector4b> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z)
			.Property(m => m.W);
}