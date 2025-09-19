using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.Bcmdl;

public class MaterialParameters : DataStructure<MaterialParameters>
{
	public float Unknown0 { get; set; }
	public uint  Unknown1 { get; set; } = 3;

	public float FloatParam0  { get; set; } = 1f;
	public float FloatParam1  { get; set; } = 1f;
	public float FloatParam2  { get; set; } = 1f;
	public float FloatParam3  { get; set; } = 1f;
	public float FloatParam4  { get; set; } = 1f;
	public float FloatParam5  { get; set; } = 1f;
	public float FloatParam6  { get; set; } = 1f;
	public float FloatParam7  { get; set; } = 1f;
	public float FloatParam8  { get; set; } = 1f;
	public float FloatParam9  { get; set; } = 1f;
	public float FloatParam10 { get; set; } = 1f;
	public float FloatParam11 { get; set; } = 1f;
	public float FloatParam12 { get; set; } = 1f;
	public float FloatParam13 { get; set; } = 1f;
	public float FloatParam14 { get; set; } = 1f;
	public float FloatParam15 { get; set; } = 1f;
	public float FloatParam16 { get; set; } = 1f;
	public float FloatParam17 { get; set; } = 1f;

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

		builder.Property(m => m.FloatParam0);
		builder.Property(m => m.FloatParam1);
		builder.Property(m => m.FloatParam2);
		builder.Property(m => m.FloatParam3);
		builder.Property(m => m.FloatParam4);
		builder.Property(m => m.FloatParam5);
		builder.Property(m => m.FloatParam6);
		builder.Property(m => m.FloatParam7);
		builder.Property(m => m.FloatParam8);
		builder.Property(m => m.FloatParam9);
		builder.Property(m => m.FloatParam10);
		builder.Property(m => m.FloatParam11);
		builder.Property(m => m.FloatParam12);
		builder.Property(m => m.FloatParam13);
		builder.Property(m => m.FloatParam14);
		builder.Property(m => m.FloatParam15);
		builder.Property(m => m.FloatParam16);
		builder.Property(m => m.FloatParam17);

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