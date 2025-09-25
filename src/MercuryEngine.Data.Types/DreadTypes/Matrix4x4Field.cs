using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Matrix4x4Field(Matrix4x4 initialValue) : BaseBinaryField<Matrix4x4>(initialValue)
{
	public Matrix4x4Field()
		: this(Matrix4x4.Identity) { }

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

	private static Matrix4x4 ReadMatrix(byte[] data)
	{
		var result = default(Matrix4x4);

		// Dread's matrices are stored in column-first order, while the Matrix4x4 struct from .NET is in row-first order.
		// This means we can't simply copy the raw byte data on top of the matrix struct.
		var values = MemoryMarshal.Cast<byte, float>(data);

		result.M11 = values[0];
		result.M21 = values[1];
		result.M31 = values[2];
		result.M41 = values[3];

		result.M12 = values[4];
		result.M22 = values[5];
		result.M32 = values[6];
		result.M42 = values[7];

		result.M13 = values[8];
		result.M23 = values[9];
		result.M33 = values[10];
		result.M43 = values[11];

		result.M14 = values[12];
		result.M24 = values[13];
		result.M34 = values[14];
		result.M44 = values[15];

		return result;
	}

	private static byte[] WriteMatrix(Matrix4x4 matrix)
	{
		var data = new byte[Unsafe.SizeOf<Matrix4x4>()];

		// Dread's matrices are stored in column-first order, while the Matrix4x4 struct from .NET is in row-first order.
		// This means we can't simply copy the raw byte data from the matrix struct.
		var values = MemoryMarshal.Cast<byte, float>(data.AsSpan());

		values[0] = matrix.M11;
		values[1] = matrix.M21;
		values[2] = matrix.M31;
		values[3] = matrix.M41;

		values[4] = matrix.M12;
		values[5] = matrix.M22;
		values[6] = matrix.M32;
		values[7] = matrix.M42;

		values[8] = matrix.M13;
		values[9] = matrix.M23;
		values[10] = matrix.M33;
		values[11] = matrix.M43;

		values[12] = matrix.M14;
		values[13] = matrix.M24;
		values[14] = matrix.M34;
		values[15] = matrix.M44;

		return data;
	}
}