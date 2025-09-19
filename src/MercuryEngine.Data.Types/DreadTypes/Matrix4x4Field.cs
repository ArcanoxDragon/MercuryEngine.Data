using System.Numerics;
using System.Runtime.CompilerServices;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Matrix4x4Field() : BaseBinaryField<Matrix4x4>(Matrix4x4.Identity)
{
	public override uint Size => (uint) Unsafe.SizeOf<Matrix4x4>();

	public override void Read(BinaryReader reader, ReadContext context)
	{
		var data = reader.ReadBytes((int) Size);

		Value = ReadMatrix(data);
	}

	public override void Write(BinaryWriter writer, WriteContext context)
	{
		var data = WriteMatrix(Value);

		writer.Write(data);
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		var data = await reader.ReadBytesAsync((int) Size, cancellationToken).ConfigureAwait(false);

		Value = ReadMatrix(data);
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		var data = WriteMatrix(Value);

		await writer.WriteAsync(data, cancellationToken).ConfigureAwait(false);
	}

	private unsafe Matrix4x4 ReadMatrix(byte[] data)
	{
		var value = default(Matrix4x4);

		fixed (void* dataPtr = data)
			Buffer.MemoryCopy(dataPtr, &value, sizeof(Matrix4x4), sizeof(Matrix4x4));

		return value;
	}

	private unsafe byte[] WriteMatrix(Matrix4x4 matrix)
	{
		var data = new byte[sizeof(Matrix4x4)];

		fixed (void* dataPtr = data)
			Buffer.MemoryCopy(&matrix, dataPtr, sizeof(Matrix4x4), sizeof(Matrix4x4));

		return data;
	}
}