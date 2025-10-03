using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

public class ShaderBindings : IBinaryField
{
	public const int BindingCount = 6;

	private readonly int[] bindings = new int[BindingCount];

	public int this[int index]
	{
		get => this.bindings[index];
		set => this.bindings[index] = value;
	}

	public uint GetSize(uint startPosition)
		=> BindingCount * sizeof(int);

	public void Read(BinaryReader reader, ReadContext context)
	{
		for (var i = 0; i < BindingCount; i++)
			this.bindings[i] = reader.ReadInt32();
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		for (var i = 0; i < BindingCount; i++)
			writer.Write(this.bindings[i]);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		for (var i = 0; i < BindingCount; i++)
			this.bindings[i] = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		for (var i = 0; i < BindingCount; i++)
			await writer.WriteAsync(this.bindings[i], cancellationToken).ConfigureAwait(false);
	}

	public override string ToString()
		=> $"[ {string.Join(", ", this.bindings)} ]";
}