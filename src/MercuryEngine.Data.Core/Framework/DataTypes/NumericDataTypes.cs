using System.Runtime.CompilerServices;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.DataTypes;

public abstract class NumericDataType<T>() : BaseDataType<T>(default)
where T : unmanaged
{
	public override uint Size => (uint) Unsafe.SizeOf<T>();
}

public class BoolDataType : NumericDataType<bool>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadBoolean();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadBooleanAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int16DataType : NumericDataType<short>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt16();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt16Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt16DataType : NumericDataType<ushort>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt16();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int32DataType : NumericDataType<int>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt32();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt32DataType : NumericDataType<uint>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt32();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int64DataType : NumericDataType<long>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt64();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt64Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt64DataType : NumericDataType<ulong>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt64();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class FloatDataType : NumericDataType<float>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadSingle();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadSingleAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class DoubleDataType : NumericDataType<double>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadDouble();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadDoubleAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class DecimalDataType : NumericDataType<decimal>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadDecimal();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadDecimalAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}