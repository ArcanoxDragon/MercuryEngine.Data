using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types;
using MercuryEngine.Data.Types.Bshdat;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Formats;

public class Bshdat : BinaryFormat<Bshdat>
{
	public override string DisplayName => "BSHDAT";

	#region Public Properties

	[JsonIgnore]
	public FileVersion Version { get; } = new(1, 19, 0);

	[JsonPropertyOrder(1)]
	public IList<ShaderProgramPair?> ShaderProgramPairs
	{
		get
		{
			ShaderProgramPairsField ??= LinkedListField.Create<ShaderProgramPair>();
			return ShaderProgramPairsField.Entries;
		}
	}

	[JsonPropertyOrder(2)]
	public IList<RenderPass?> RenderPasses
	{
		get
		{
			RenderPassesField ??= LinkedListField.Create<RenderPass>();
			return RenderPassesField.Entries;
		}
	}

	[JsonPropertyOrder(3)]
	public ShaderProgramPair? OutputShader { get; set; }

	[JsonPropertyOrder(4)]
	public IList<ShaderVariable?> VertexShaderUniforms
	{
		get
		{
			VertexShaderUniformsField ??= CreateShaderInputField();
			return VertexShaderUniformsField.Entries;
		}
	}

	[JsonPropertyOrder(5)]
	public IList<ShaderVariable?> FragmentShaderUniforms
	{
		get
		{
			FragmentShaderUniformsField ??= CreateShaderInputField();
			return FragmentShaderUniformsField.Entries;
		}
	}

	[JsonPropertyOrder(6)]
	public IList<ShaderVariable?> VertexShaderInputs
	{
		get
		{
			VertexShaderInputsField ??= CreateShaderInputField();
			return VertexShaderInputsField.Entries;
		}
	}

	#endregion

	#region Private Data

	private LinkedListField<ShaderProgramPair>? ShaderProgramPairsField     { get; set; } = LinkedListField.Create<ShaderProgramPair>();
	private LinkedListField<RenderPass>?        RenderPassesField           { get; set; } = LinkedListField.Create<RenderPass>();
	private LinkedListField<ShaderVariable>?    VertexShaderUniformsField   { get; set; } = CreateShaderInputField();
	private LinkedListField<ShaderVariable>?    FragmentShaderUniformsField { get; set; } = CreateShaderInputField();
	private LinkedListField<ShaderVariable>?    VertexShaderInputsField     { get; set; } = CreateShaderInputField();

	#endregion

	#region Hooks

	private static LinkedListField<ShaderVariable> CreateShaderInputField()
		=> LinkedListField.Create<ShaderVariable>(startByteAlignment: ShaderVariable.StartAlignment);

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		context.HeapManager.PaddingByte = 0xFF;

		// The allocation order of BSHDAT is non-standard (pointers are allocated in a different order than that in which they appear).
		// To ensure correct write-back, we need to manually allocate addressed for fields in the desired allocation order, which will
		// then be re-used when writing in the natural pointer order (none of the pointers are "unique").

		// All program data comes first
		foreach (var programPair in ShaderProgramPairs)
			programPair?.AllocateSpaceForPrograms(context);

		// All linked lists come next
		if (ShaderProgramPairsField != null)
			context.HeapManager.Allocate(ShaderProgramPairsField);
		if (RenderPassesField != null)
			context.HeapManager.Allocate(RenderPassesField);
		if (VertexShaderUniformsField != null)
			context.HeapManager.Allocate(VertexShaderUniformsField);
		if (FragmentShaderUniformsField != null)
			context.HeapManager.Allocate(FragmentShaderUniformsField);
		if (VertexShaderInputsField != null)
			context.HeapManager.Allocate(VertexShaderInputsField);

		// All shader program pairs come next (in the order of the list, not including "OutputShader"!)
		foreach (var programPair in ShaderProgramPairs)
		{
			if (programPair != null)
				context.HeapManager.Allocate(programPair);
		}

		// The rest of the allocations can appear in natural order
	}

	#endregion

	protected override void Describe(DataStructureBuilder<Bshdat> builder)
	{
		builder.Constant("MSHD", "<magic>", terminated: false);
		builder.RawProperty(m => m.Version);
		builder.Pointer(m => m.ShaderProgramPairsField, _ => LinkedListField.Create<ShaderProgramPair>());
		builder.Pointer(m => m.RenderPassesField, _ => LinkedListField.Create<RenderPass>());
		builder.Pointer(m => m.OutputShader);
		builder.Padding(8);
		builder.Pointer(m => m.VertexShaderUniformsField, _ => CreateShaderInputField());
		builder.Pointer(m => m.FragmentShaderUniformsField, _ => CreateShaderInputField());
		builder.Pointer(m => m.VertexShaderInputsField, _ => CreateShaderInputField());
	}
}