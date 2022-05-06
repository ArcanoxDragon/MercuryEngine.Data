using System.Text;
using MercuryEngine.Data.Exceptions;
using MercuryEngine.Data.Extensions;

namespace MercuryEngine.Data.Framework.Components;

public class LiteralComponent : BinaryComponent, IFixedSizeBinaryComponent
{
	private readonly byte[] literalData;
	private readonly bool   isTextData;

	public LiteralComponent(byte[] literalData)
	{
		this.literalData = literalData;
	}

	public LiteralComponent(string literalText) : this(Encoding.UTF8.GetBytes(literalText))
	{
		this.isTextData = true;
	}

	public override bool IsFixedSize => true;

	public uint Size => (uint) this.literalData.Length;

	public override bool Validate(Stream stream)
		=> stream.HasBytes(Size);

	public override object Read(BinaryReader reader)
	{
		var offset = reader.BaseStream.Position;
		var buffer = new byte[Size];
		var read = reader.Read(buffer);

		if (read < Size)
			throw new DataValidationException(offset, $"Expected to read {Size} bytes, but only got {read}");

		for (var i = 0; i < Size; i++)
		{
			if (buffer[i] != this.literalData[i])
				throw new DataValidationException(offset, $"Data mismatch at byte {i} of sequence: expected \"{Format(this.literalData)}\" but got \"{Format(buffer)}\"");
		}

		return this.isTextData ? Encoding.UTF8.GetString(buffer) : this.literalData;
	}

	public override void Write(BinaryWriter writer, object data)
	{
		// Just write our literal data byte-for-byte
		writer.Write(this.literalData);
	}

	private string Format(byte[] data)
		=> this.isTextData ? Encoding.UTF8.GetString(data) : data.ToHexString();
}