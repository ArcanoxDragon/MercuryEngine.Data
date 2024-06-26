using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

public abstract class NumberField<T>(T value) : BaseBinaryField<T>(value)
where T : unmanaged
{
	[JsonIgnore]
	public override uint Size => (uint) Unsafe.SizeOf<T>();
}

public class BooleanField(bool value) : NumberField<bool>(value)
{
	public BooleanField() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadBoolean();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadBooleanAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int16Field(short value) : NumberField<short>(value)
{
	public Int16Field() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt16();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt16Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt16Field(ushort value) : NumberField<ushort>(value)
{
	public UInt16Field() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt16();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int32Field(int value) : NumberField<int>(value)
{
	public Int32Field() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt32();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt32Field(uint value) : NumberField<uint>(value)
{
	public UInt32Field() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt32();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class Int64Field(long value) : NumberField<long>(value)
{
	public Int64Field() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt64();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadInt64Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class UInt64Field(ulong value) : NumberField<ulong>(value)
{
	public UInt64Field() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt64();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class FloatField(float value) : NumberField<float>(value)
{
	public FloatField() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadSingle();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadSingleAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class DoubleField(double value) : NumberField<double>(value)
{
	public DoubleField() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadDouble();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadDoubleAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}

public class DecimalField(decimal value) : NumberField<decimal>(value)
{
	public DecimalField() : this(default) { }

	public override void Read(BinaryReader reader)
		=> Value = reader.ReadDecimal();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> Value = await reader.ReadDecimalAsync(cancellationToken).ConfigureAwait(false);

	public override Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> writer.WriteAsync(Value, cancellationToken);
}