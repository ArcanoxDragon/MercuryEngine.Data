using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public class ReflectionSectionHeader : DataSectionHeader<ReflectionSectionHeader>
{
	internal ReflectionSectionHeader(DataSection parentSection) : base(parentSection) { }

	public UniformBlock[]   UniformBlocks   { get; private set; } = [];
	public Uniform[]        Uniforms        { get; private set; } = [];
	public ShaderInput[]    Inputs          { get; private set; } = [];
	public ShaderOutput[]   Outputs         { get; private set; } = [];
	public StorageBuffer[]  StorageBuffers  { get; private set; } = [];
	public BufferVariable[] BufferVariables { get; private set; } = [];
	public Varying[]        Varyings        { get; private set; } = [];

	#region Private Data

	private uint UniformBlocksCount  { get; set; }
	private uint UniformBlocksOffset { get; set; }

	private uint UniformsCount  { get; set; }
	private uint UniformsOffset { get; set; }

	private uint InputsCount  { get; set; }
	private uint InputsOffset { get; set; }

	private uint OutputsCount  { get; set; }
	private uint OutputsOffset { get; set; }

	private uint StorageBuffersCount  { get; set; }
	private uint StorageBuffersOffset { get; set; }

	private uint BufferVariablesCount  { get; set; }
	private uint BufferVariablesOffset { get; set; }

	private uint VaryingsCount  { get; set; }
	private uint VaryingsOffset { get; set; }

	internal uint StringTableSize   { get; set; }
	internal uint StringTableOffset { get; set; }

	private uint UnknownOffset1 { get; set; }

	private uint UnknownCount2  { get; set; }
	private uint UnknownOffset2 { get; set; }

	private uint UnknownCount3  { get; set; }
	private uint UnknownOffset3 { get; set; }

	private uint UnknownCount4  { get; set; }
	private uint UnknownOffset4 { get; set; }

	#endregion

	protected override void ReadData(BinaryReader reader, ReadContext context)
	{
		UniformBlocks = ReadArray(UniformBlocksCount, UniformBlocksOffset, () => new UniformBlock(this));
		Uniforms = ReadArray(UniformsCount, UniformsOffset, () => new Uniform(this));
		Inputs = ReadArray(InputsCount, InputsOffset, () => new ShaderInput(this));
		Outputs = ReadArray(OutputsCount, OutputsOffset, () => new ShaderOutput(this));
		StorageBuffers = ReadArray(StorageBuffersCount, StorageBuffersOffset, () => new StorageBuffer(this));
		BufferVariables = ReadArray(BufferVariablesCount, BufferVariablesOffset, () => new BufferVariable(this));
		Varyings = ReadArray(VaryingsCount, VaryingsOffset, () => new Varying(this));

		T[] ReadArray<T>(uint count, uint offset, Func<T> factory)
		where T : IBinaryField
		{
			if (count == 0 || offset == 0)
				return [];

			var array = new T[count];
			var finalOffset = ParentSection.DataOffset + offset;

			using (reader.BaseStream.TemporarySeek(finalOffset))
			{
				for (var i = 0; i < count; i++)
				{
					array[i] = factory();
					array[i].Read(reader, context);
				}
			}

			return array;
		}
	}

	protected override async Task ReadDataAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		UniformBlocks = await ReadArrayAsync(UniformBlocksCount, UniformBlocksOffset, () => new UniformBlock(this)).ConfigureAwait(false);
		Uniforms = await ReadArrayAsync(UniformsCount, UniformsOffset, () => new Uniform(this)).ConfigureAwait(false);
		Inputs = await ReadArrayAsync(InputsCount, InputsOffset, () => new ShaderInput(this)).ConfigureAwait(false);
		Outputs = await ReadArrayAsync(OutputsCount, OutputsOffset, () => new ShaderOutput(this)).ConfigureAwait(false);
		StorageBuffers = await ReadArrayAsync(StorageBuffersCount, StorageBuffersOffset, () => new StorageBuffer(this)).ConfigureAwait(false);
		BufferVariables = await ReadArrayAsync(BufferVariablesCount, BufferVariablesOffset, () => new BufferVariable(this)).ConfigureAwait(false);
		Varyings = await ReadArrayAsync(VaryingsCount, VaryingsOffset, () => new Varying(this)).ConfigureAwait(false);

		async ValueTask<T[]> ReadArrayAsync<T>(uint count, uint offset, Func<T> factory)
		where T : IBinaryField
		{
			if (count == 0 || offset == 0)
				return [];

			var array = new T[count];
			var finalOffset = ParentSection.DataOffset + offset;

			using (reader.BaseStream.TemporarySeek(finalOffset))
			{
				for (var i = 0; i < count; i++)
				{
					array[i] = factory();
					await array[i].ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
				}
			}

			return array;
		}
	}

	protected override void Describe(DataStructureBuilder<ReflectionSectionHeader> builder)
	{
		builder.Property(m => m.UniformBlocksCount);
		builder.Property(m => m.UniformBlocksOffset);
		builder.Property(m => m.UniformsCount);
		builder.Property(m => m.UniformsOffset);
		builder.Property(m => m.InputsCount);
		builder.Property(m => m.InputsOffset);
		builder.Property(m => m.OutputsCount);
		builder.Property(m => m.OutputsOffset);
		builder.Property(m => m.StorageBuffersCount);
		builder.Property(m => m.StorageBuffersOffset);
		builder.Property(m => m.BufferVariablesCount);
		builder.Property(m => m.BufferVariablesOffset);
		builder.Property(m => m.VaryingsCount);
		builder.Property(m => m.VaryingsOffset);
		builder.Property(m => m.StringTableSize);
		builder.Property(m => m.StringTableOffset);
		builder.Property(m => m.UnknownOffset1);
		builder.Property(m => m.UnknownCount2);
		builder.Property(m => m.UnknownOffset2);
		builder.Property(m => m.UnknownCount3);
		builder.Property(m => m.UnknownOffset3);
		builder.Property(m => m.UnknownCount4);
		builder.Property(m => m.UnknownOffset4);
		builder.Padding(8); // To make entire section 144 bytes
	}
}