using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.Bcmdl;

public class MaterialParameters : DataStructure<MaterialParameters>
{
	public float Unknown0 { get; set; }
	public uint  Unknown1 { get; set; } = 3;

	public Vector3 Vec3Param0 { get; } = new(1f, 1f, 1f);
	public Vector3 Vec3Param1 { get; } = new(1f, 1f, 1f);
	public Vector3 Vec3Param2 { get; } = new(1f, 1f, 1f);
	public Vector3 Vec3Param3 { get; } = new(1f, 1f, 1f);
	public Vector3 Vec3Param4 { get; } = new(1f, 1f, 1f);
	public Vector3 Vec3Param5 { get; } = new(1f, 1f, 1f);

	public Vector4 Vec4Param0 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param1 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param2 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param3 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param4 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param5 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param6 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param7 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param8 { get; } = new(0f, 0f, 0f, 1f);
	public Vector4 Vec4Param9 { get; } = new(0f, 0f, 0f, 1f);

	public uint ParamIndex0 { get; set; }
	public uint ParamIndex1 { get; set; } = 1;
	public uint ParamIndex2 { get; set; } = 2;
	public uint ParamIndex3 { get; set; } = 3;
	public uint ParamIndex4 { get; set; } = 4;
	public uint ParamIndex5 { get; set; } = 5;
	public uint ParamIndex6 { get; set; } = 6;
	public uint ParamIndex7 { get; set; } = 7;
	public uint ParamIndex8 { get; set; } = 8;
	public uint ParamIndex9 { get; set; } = 9;

	protected override void Describe(DataStructureBuilder<MaterialParameters> builder)
	{
		builder.Property(m => m.Unknown0);
		builder.Property(m => m.Unknown1);

		builder.RawProperty(m => m.Vec3Param0);
		builder.RawProperty(m => m.Vec3Param1);
		builder.RawProperty(m => m.Vec3Param2);
		builder.RawProperty(m => m.Vec3Param3);
		builder.RawProperty(m => m.Vec3Param4);
		builder.RawProperty(m => m.Vec3Param5);

		builder.RawProperty(m => m.Vec4Param0);
		builder.RawProperty(m => m.Vec4Param1);
		builder.RawProperty(m => m.Vec4Param2);
		builder.RawProperty(m => m.Vec4Param3);
		builder.RawProperty(m => m.Vec4Param4);
		builder.RawProperty(m => m.Vec4Param5);
		builder.RawProperty(m => m.Vec4Param6);
		builder.RawProperty(m => m.Vec4Param7);
		builder.RawProperty(m => m.Vec4Param8);
		builder.RawProperty(m => m.Vec4Param9);

		builder.Property(m => m.ParamIndex0);
		builder.Property(m => m.ParamIndex1);
		builder.Property(m => m.ParamIndex2);
		builder.Property(m => m.ParamIndex3);
		builder.Property(m => m.ParamIndex4);
		builder.Property(m => m.ParamIndex5);
		builder.Property(m => m.ParamIndex6);
		builder.Property(m => m.ParamIndex7);
		builder.Property(m => m.ParamIndex8);
		builder.Property(m => m.ParamIndex9);
	}
}