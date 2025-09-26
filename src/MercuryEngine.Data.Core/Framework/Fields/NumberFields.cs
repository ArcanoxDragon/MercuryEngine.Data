using System.Runtime.CompilerServices;
using System.Text;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

// ReSharper disable PreferConcreteValueOverDefault

namespace MercuryEngine.Data.Core.Framework.Fields;

public abstract class NumberField<T>(T value) : BaseBinaryField<T>(value)
where T : unmanaged
{
	public override uint GetSize(uint startPosition) => (uint) Unsafe.SizeOf<T>();
}

public class BooleanField(bool value) : NumberField<bool>(value)
{
	public BooleanField() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadBoolean();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadBooleanAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class ByteField(byte value) : NumberField<byte>(value)
{
	public ByteField() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadByte();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class CharField(char value) : NumberField<char>(value)
{
	// TODO: This assumes that UTF-8 is always being used for writing, which is *probably* the case, but
	// the more robust solution would be to turn the Size property into "GetSize" which somehow passes an encoding
	private static readonly UTF8Encoding UnicodeEncoding = new(false);

	public CharField() : this(default) { }

	public override uint GetSize(uint startPosition) => (uint) UnicodeEncoding.GetByteCount([Value]);

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadChar();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int16Field(short value) : NumberField<short>(value)
{
	public Int16Field() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadInt16();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt16Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt16Field(ushort value) : NumberField<ushort>(value)
{
	public UInt16Field() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadUInt16();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int32Field(int value) : NumberField<int>(value)
{
	public Int32Field() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadInt32();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt32Field(uint value) : NumberField<uint>(value)
{
	public UInt32Field() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadUInt32();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int64Field(long value) : NumberField<long>(value)
{
	public Int64Field() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadInt64();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt64Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt64Field(ulong value) : NumberField<ulong>(value)
{
	public UInt64Field() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadUInt64();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class FloatField(float value) : NumberField<float>(value)
{
	public FloatField() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadSingle();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadSingleAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class DoubleField(double value) : NumberField<double>(value)
{
	public DoubleField() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadDouble();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadDoubleAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class DecimalField(decimal value) : NumberField<decimal>(value)
{
	public DecimalField() : this(default) { }

	public override void Read(BinaryReader reader, ReadContext context)
		=> Value = reader.ReadDecimal();

	public override void Write(BinaryWriter writer, WriteContext context)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadDecimalAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}