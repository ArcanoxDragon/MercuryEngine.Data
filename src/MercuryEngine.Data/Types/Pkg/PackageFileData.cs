using System.Text;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Pkg;

internal class PackageFileData : DataStructure<PackageFileData>
{
	private const uint DefaultStartAlignment = 4;
	private const uint EndAlignment          = 8;

	public PackageFileData(PackageFile owner)
	{
		Owner = owner;
		DataField = new RawBytes(() => (int) ( EndAddress - StartAddress ));
	}

	public uint StartAddress { get; set; }
	public uint EndAddress   { get; set; }

	public byte[] Data
	{
		get => DataField.Value;
		set => DataField.Value = value;
	}

	private PackageFile Owner     { get; }
	private RawBytes    DataField { get; }

	protected override void ReadCore(BinaryReader reader, ReadContext context)
	{
		base.ReadCore(reader, context);

		// Read the file's data
		var currentPosition = reader.BaseStream.Position;

		reader.BaseStream.Position = StartAddress;
		DataField.Read(reader, context);
		context.HeapManager.Register(StartAddress, DataField, endByteAlignment: EndAlignment);
		reader.BaseStream.Position = currentPosition;
	}

	protected override void WriteCore(BinaryWriter writer, WriteContext context)
	{
		// Allocate space for the file data, and update our addresses before writing
		var description = $"File data for {Owner.Name}";

		StartAddress = (uint) context.HeapManager.Allocate(DataField, startByteAlignment: GetStartAlignment(), endByteAlignment: EndAlignment, description: description);
		EndAddress = StartAddress + (uint) Data.Length;

		base.WriteCore(writer, context);
	}

	protected override async Task ReadAsyncCore(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await base.ReadAsyncCore(reader, context, cancellationToken).ConfigureAwait(false);

		// Read the file's data
		var currentPosition = reader.BaseStream.Position;

		reader.BaseStream.Position = StartAddress;
		await DataField.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		context.HeapManager.Register(StartAddress, DataField, endByteAlignment: EndAlignment);
		reader.BaseStream.Position = currentPosition;
	}

	protected override Task WriteAsyncCore(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		// Allocate space for the file data, and update our addresses before writing
		var description = $"File data for {Owner.Name}";

		StartAddress = (uint) context.HeapManager.Allocate(DataField, startByteAlignment: GetStartAlignment(), endByteAlignment: EndAlignment, description: description);
		EndAddress = StartAddress + (uint) Data.Length;

		return base.WriteAsyncCore(writer, context, cancellationToken);
	}

	private uint GetStartAlignment()
	{
		var fileFormat = Data.Length < 4 ? null : Encoding.ASCII.GetString(Data[..4]);

		return fileFormat switch {
			"CWAV" => 32,
			"LSND" => 16,
			"MTUN" => 128,
			"MTXT" => 128,
			_      => DefaultStartAlignment,
		};
	}

	protected override void Describe(DataStructureBuilder<PackageFileData> builder)
	{
		builder.Property(m => m.StartAddress);
		builder.Property(m => m.EndAddress);
	}
}