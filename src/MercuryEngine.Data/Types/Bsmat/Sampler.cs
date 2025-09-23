using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.Bsmat;

[DebuggerDisplay("Name = {Name}, TexturePath = {TexturePath}")]
public class Sampler : DataStructure<Sampler>
{
	public string      Name                { get; set; } = string.Empty;
	public string      ShaderBindingName   { get; set; } = string.Empty;
	public string      Type                { get; set; } = string.Empty;
	public int         Index               { get; set; }
	public string      TexturePath         { get; set; } = string.Empty;
	public FilterMode  MinificationFilter  { get; set; }
	public FilterMode  MagnificationFilter { get; set; }
	public FilterMode  MipmapFilter        { get; set; }
	public CompareMode CompareMode         { get; set; }
	public TilingMode  TilingModeU         { get; set; }
	public TilingMode  TilingModeV         { get; set; }
	public Vector4b    BorderColor         { get; set; } = new();
	public float       MinLevelOfDetail    { get; set; }
	public float       LevelOfDetailBias   { get; set; }
	public float       Anisotropic         { get; set; }
	public float       MaxMipLevel         { get; set; }
	public float       MaxAnisotropy       { get; set; }

	protected override void Describe(DataStructureBuilder<Sampler> builder)
	{
		builder.Property(m => m.Name);
		builder.Property(m => m.ShaderBindingName);
		builder.Property(m => m.Type);
		builder.Property(m => m.Index);
		builder.Property(m => m.TexturePath);
		builder.Property(m => m.MinificationFilter);
		builder.Property(m => m.MagnificationFilter);
		builder.Property(m => m.MipmapFilter);
		builder.Property(m => m.CompareMode);
		builder.Property(m => m.TilingModeU);
		builder.Property(m => m.TilingModeV);
		builder.RawProperty(m => m.BorderColor);
		builder.Property(m => m.MinLevelOfDetail);
		builder.Property(m => m.LevelOfDetailBias);
		builder.Property(m => m.Anisotropic);
		builder.Property(m => m.MaxMipLevel);
		builder.Property(m => m.MaxAnisotropy);
	}
}