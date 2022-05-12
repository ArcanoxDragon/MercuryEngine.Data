using System.Runtime.CompilerServices;

namespace MercuryEngine.Data.Core.Framework.DataTypes;

public abstract class NumericDataType<T> : BaseDataType<T>
where T : unmanaged
{
	protected NumericDataType() : base(default) { }

	public override uint Size => (uint) Unsafe.SizeOf<T>();
}

public class BoolDataType : NumericDataType<bool>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadBoolean();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class Int16DataType : NumericDataType<short>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt16();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class UInt16DataType : NumericDataType<ushort>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt16();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class Int32DataType : NumericDataType<int>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt32();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class UInt32DataType : NumericDataType<uint>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt32();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class Int64DataType : NumericDataType<long>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadInt64();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class UInt64DataType : NumericDataType<ulong>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadUInt64();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class FloatDataType : NumericDataType<float>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadSingle();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class DoubleDataType : NumericDataType<double>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadDouble();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}

public class DecimalDataType : NumericDataType<decimal>
{
	public override void Read(BinaryReader reader)
		=> Value = reader.ReadDecimal();

	public override void Write(BinaryWriter writer)
		=> writer.Write(Value);
}