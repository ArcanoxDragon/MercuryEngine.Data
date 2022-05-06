namespace MercuryEngine.Data.Framework.Components;

public class Int16Component : FixedSizeBinaryComponent<short>
{
	public override uint Size => sizeof(short);

	public override short Read(BinaryReader reader)
		=> reader.ReadInt16();

	public override void Write(BinaryWriter writer, short data)
		=> writer.Write(data);
}

public class UInt16Component : FixedSizeBinaryComponent<ushort>
{
	public override uint Size => sizeof(ushort);

	public override ushort Read(BinaryReader reader)
		=> reader.ReadUInt16();

	public override void Write(BinaryWriter writer, ushort data)
		=> writer.Write(data);
}

public class Int32Component : FixedSizeBinaryComponent<int>
{
	public override uint Size => sizeof(int);

	public override int Read(BinaryReader reader)
		=> reader.ReadInt32();

	public override void Write(BinaryWriter writer, int data)
		=> writer.Write(data);
}

public class UInt32Component : FixedSizeBinaryComponent<uint>
{
	public override uint Size => sizeof(uint);

	public override uint Read(BinaryReader reader)
		=> reader.ReadUInt16();

	public override void Write(BinaryWriter writer, uint data)
		=> writer.Write(data);
}

public class Int64Component : FixedSizeBinaryComponent<long>
{
	public override uint Size => sizeof(long);

	public override long Read(BinaryReader reader)
		=> reader.ReadInt64();

	public override void Write(BinaryWriter writer, long data)
		=> writer.Write(data);
}

public class UInt64Component : FixedSizeBinaryComponent<ulong>
{
	public override uint Size => sizeof(ulong);

	public override ulong Read(BinaryReader reader)
		=> reader.ReadUInt64();

	public override void Write(BinaryWriter writer, ulong data)
		=> writer.Write(data);
}

public class FloatComponent : FixedSizeBinaryComponent<float>
{
	public override uint Size => sizeof(float);

	public override float Read(BinaryReader reader)
		=> reader.ReadSingle();

	public override void Write(BinaryWriter writer, float data)
		=> writer.Write(data);
}

public class DoubleComponent : FixedSizeBinaryComponent<double>
{
	public override uint Size => sizeof(double);

	public override double Read(BinaryReader reader)
		=> reader.ReadDouble();

	public override void Write(BinaryWriter writer, double data)
		=> writer.Write(data);
}

public class DecimalComponent : FixedSizeBinaryComponent<decimal>
{
	public override uint Size => sizeof(decimal);

	public override decimal Read(BinaryReader reader)
		=> reader.ReadDecimal();

	public override void Write(BinaryWriter writer, decimal data)
		=> writer.Write(data);
}