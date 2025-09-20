using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class UnknownMaterialParams : DataStructure<UnknownMaterialParams>
{
	public Material?    Material        { get; set; }
	public UnknownFlag? Flag            { get; set; }
	public uint         UnknownU32_0x10 { get; set; }
	public uint         UnknownU32_0x14 { get; set; }
	public float        UnknownS32_0x18 { get; set; }
	public uint         UnknownU32_0x1C { get; set; }
	public uint         UnknownU32_0x20 { get; set; }

	public Inner1? Inner1_1 { get; set; }
	public Inner2? Inner2_1 { get; set; }
	public Inner1? Inner1_2 { get; set; }

	// public Ref<Inner1>  Inner1_1        { get; } = new();
	// public Ref<Inner2>  Inner2_1        { get; } = new();
	// public Ref<Inner1>  Inner1_2        { get; } = new();

	protected override void Describe(DataStructureBuilder<UnknownMaterialParams> builder)
	{
		builder.Pointer(m => m.Material);
		builder.Pointer(m => m.Flag);
		builder.Property(m => m.UnknownU32_0x10);
		builder.Property(m => m.UnknownU32_0x14);
		builder.Property(m => m.UnknownS32_0x18);
		builder.Property(m => m.UnknownU32_0x1C);
		builder.Property(m => m.UnknownU32_0x20);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.Inner1_1, startByteAlignment: 8);
		builder.Pointer(m => m.Inner2_1, startByteAlignment: 8);
		builder.Pointer(m => m.Inner1_2, startByteAlignment: 8);
		// builder.RawProperty(m => m.Inner1_1Conditional);
		// builder.RawProperty(m => m.Inner2_1Conditional);
		// builder.RawProperty(m => m.Inner1_2Conditional);
	}

	public sealed class UnknownFlag : DataStructure<UnknownFlag>
	{
		public bool Flag { get; set; }

		protected override void Describe(DataStructureBuilder<UnknownFlag> builder)
		{
			builder.Property(m => m.Flag);
		}
	}

	public sealed class Ref<T> : DataStructure<Ref<T>>
	where T : class, IBinaryField, new()
	{
		public T? Target { get; set; }

		protected override void Describe(DataStructureBuilder<Ref<T>> builder)
		{
			builder.Pointer(m => m.Target);
		}
	}

	public sealed class Inner1 : DataStructure<Inner1>
	{
		public Inner3? Inner3_1 { get; set; }
		public Inner3? Inner3_2 { get; set; }

		protected override void Describe(DataStructureBuilder<Inner1> builder)
		{
			builder.Pointer(m => m.Inner3_1, startByteAlignment: 8);
			builder.Pointer(m => m.Inner3_2, startByteAlignment: 8);
		}
	}

	public sealed class Inner2 : DataStructure<Inner2>
	{
		public Inner3? Inner3 { get; set; }

		protected override void Describe(DataStructureBuilder<Inner2> builder)
		{
			builder.Pointer(m => m.Inner3, startByteAlignment: 8);
		}
	}

	public sealed class Inner3 : DataStructure<Inner3>
	{
		public uint  Count      { get; set; }
		public uint  ByteLength { get; set; }
		public uint  DataType   { get; set; }
		public uint  Unk1       { get; set; }
		public float Unk2       { get; set; }

		private byte[] BufferData
		{
			get => BufferDataField?.Value ?? [];
			set
			{
				BufferDataField ??= CreateRawDataField();
				BufferDataField.Value = value;
				ByteLength = (uint) value.Length;
			}
		}

		#region Private Data

		private RawBytes? BufferDataField { get; set; }

		#endregion

		private RawBytes CreateRawDataField()
			=> new(() => (int) ( ByteLength * Count ));

		protected override void Describe(DataStructureBuilder<Inner3> builder)
		{
			builder.Property(m => m.Count);
			builder.Property(m => m.ByteLength);
			builder.Property(m => m.DataType);
			builder.Property(m => m.Unk1);
			builder.Property(m => m.Unk2);
			builder.Padding(4, 0xFF);
			builder.Pointer(m => m.BufferDataField, owner => owner.CreateRawDataField(), startByteAlignment: 8, endByteAlignment: 8);
		}
	}
}