using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat;

public class ShaderProgramPair : DataStructure<ShaderProgramPair>
{
	public const uint ProgramDataAlignment = 0x80;

	#region Public Properties

	public byte[] VertexProgramData   => VertexProgramDataField?.Value ?? [];
	public byte[] FragmentProgramData => FragmentProgramDataField?.Value ?? [];

	#endregion

	#region Private Data

	private uint Unknown1 { get; set; }
	private uint Unknown2 { get; set; }
	private uint Unknown3 { get; set; }

	private uint      VertexProgramSize      { get; set; }
	private RawBytes? VertexProgramDataField { get; set; }

	private uint      FragmentProgramSize      { get; set; }
	private RawBytes? FragmentProgramDataField { get; set; }

	#endregion

	#region Hooks

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		VertexProgramSize = (uint) ( VertexProgramDataField?.Value.Length ?? 0 );
		FragmentProgramSize = (uint) ( FragmentProgramDataField?.Value.Length ?? 0 );
	}

	#endregion

	internal void AllocateSpaceForPrograms(WriteContext context)
	{
		if (VertexProgramDataField != null)
			context.HeapManager.Allocate(VertexProgramDataField, startByteAlignment: ProgramDataAlignment, description: "Vertex Program Data");
		if (FragmentProgramDataField != null)
			context.HeapManager.Allocate(FragmentProgramDataField, startByteAlignment: ProgramDataAlignment, description: "Fragment Program Data");
	}

	private RawBytes CreateVertexProgramDataField()
		=> new(() => (int) VertexProgramSize);

	private RawBytes CreateFragmentProgramDataField()
		=> new(() => (int) FragmentProgramSize);

	protected override void Describe(DataStructureBuilder<ShaderProgramPair> builder)
	{
		builder.Property(m => m.Unknown1);
		builder.Padding(4, 0xFF);
		builder.Property(m => m.Unknown2);
		builder.Property(m => m.Unknown3);
		builder.Property(m => m.VertexProgramSize);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.VertexProgramDataField, owner => owner.CreateVertexProgramDataField(), startByteAlignment: ProgramDataAlignment);
		builder.Property(m => m.FragmentProgramSize);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.FragmentProgramDataField, owner => owner.CreateFragmentProgramDataField(), startByteAlignment: ProgramDataAlignment);
	}
}