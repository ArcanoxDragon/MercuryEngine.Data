using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bsmat;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Formats;

public class Bsmat : BinaryFormat<Bsmat>
{
	[JsonIgnore]
	public override string DisplayName => "BSMAT";

	public FileVersion     Version      { get; }      = new(2, 17, 0);
	public string          Name         { get; set; } = string.Empty;
	public MaterialType    Type         { get; set; } = MaterialType.Unknown1;
	public int             RenderLayer  { get; set; }
	public string          ShaderPath   { get; set; } = string.Empty;
	public BlendState      BlendState   { get; set; } = new();
	public PolygonCullMode CullMode     { get; set; } = PolygonCullMode.Front; // TODO: ?? Really? The default is front-face culling? Maybe the enum is swapped
	public StencilState    StencilState { get; set; } = new();
	public AlphaState      AlphaState   { get; set; } = new();
	public FillMode        FillMode     { get; set; } = FillMode.Solid;
	public DepthState      DepthState   { get; set; } = new();

	private uint Unknown0 { get; set; }
	private uint Unknown1 { get; set; }
	private uint Unknown2 { get; set; }

	public List<UniformParameter> AdditionalUniforms { get; } = [];
	public List<Sampler>          AdditionalSamplers { get; } = [];
	public List<ShaderStage>      ShaderStages       { get; } = [];

	protected override void Describe(DataStructureBuilder<Bsmat> builder)
	{
		builder.Constant("MSUR", "<magic>", terminated: false);
		builder.RawProperty(m => m.Version);
		builder.Property(m => m.Name);
		builder.Property(m => m.Type);
		builder.Property(m => m.RenderLayer);
		builder.Property(m => m.ShaderPath);
		builder.RawProperty(m => m.BlendState);
		builder.Property(m => m.CullMode);
		builder.RawProperty(m => m.StencilState);
		builder.RawProperty(m => m.AlphaState);
		builder.Property(m => m.FillMode);
		builder.RawProperty(m => m.DepthState);

		//builder.Property(m => m.Unknown0);
		//builder.Property(m => m.Unknown1);
		//builder.Property(m => m.Unknown2);

		builder.Constant(0); // Unknown purpose
		builder.Constant(2); // Unknown purpose
		builder.Constant(0); // Unknown purpose
		builder.Array(m => m.AdditionalUniforms);
		builder.Array(m => m.AdditionalSamplers);
		builder.Array(m => m.ShaderStages);
	}
}