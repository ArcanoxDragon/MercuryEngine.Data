using System.Buffers;
using System.Text;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

internal class ReflectionName(ReflectionSectionHeader reflectionHeader) : DataStructure<ReflectionName>
{
	public uint Offset { get; set; }
	public uint Length { get; set; }

	public string Name { get; private set; } = string.Empty;

	protected override void ReadCore(BinaryReader reader, ReadContext context)
	{
		base.ReadCore(reader, context);

		if (Offset > 0 && Length > 0)
		{
			var finalOffset = reflectionHeader.ParentSection.DataOffset + reflectionHeader.StringTableOffset + Offset;
			Span<byte> nameBytes = stackalloc byte[(int) Length];

			using (reader.BaseStream.TemporarySeek(finalOffset))
				reader.BaseStream.ReadExactly(nameBytes);

			if (nameBytes[^1] == 0)
				nameBytes = nameBytes[..^1];

			Name = Encoding.UTF8.GetString(nameBytes);
		}
		else
		{
			Name = string.Empty;
		}
	}

	protected override async Task ReadAsyncCore(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await base.ReadAsyncCore(reader, context, cancellationToken).ConfigureAwait(false);

		if (Offset > 0 && Length > 0)
		{
			var finalOffset = reflectionHeader.ParentSection.DataOffset + reflectionHeader.StringTableOffset + Offset;
			var intLength = (int) Length;
			IMemoryOwner<byte> nameMemory = MemoryPool<byte>.Shared.Rent(intLength);

			using (reader.BaseStream.TemporarySeek(finalOffset))
				await reader.BaseStream.ReadExactlyAsync(nameMemory.Memory, cancellationToken).ConfigureAwait(false);

			if (nameMemory.Memory.Span[^1] == 0)
				intLength--;

			Name = Encoding.UTF8.GetString(nameMemory.Memory.Span[..intLength]);
		}
		else
		{
			Name = string.Empty;
		}
	}

	protected override void Describe(DataStructureBuilder<ReflectionName> builder)
	{
		builder.Property(m => m.Offset);
		builder.Property(m => m.Length);
		builder.Padding(0x20);
	}
}