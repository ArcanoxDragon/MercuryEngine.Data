using System.Text;
using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Core.Framework;

public abstract class BinaryFormat<T> : DataStructure<T>
where T : BinaryFormat<T>
{
	private static readonly Encoding DefaultEncoding = new UTF8Encoding();

	/// <summary>
	/// Gets the display name of this <see cref="BinaryFormat{T}"/>.
	/// </summary>
	public abstract string DisplayName { get; }

	public void Read(Stream stream)
		=> Read(stream, DefaultEncoding);

	public void Read(Stream stream, Encoding encoding)
	{
		using var reader = new BinaryReader(stream, encoding, true);

		Read(reader);

		if (stream.Position != stream.Length)
			throw new IOException($"There were {stream.Length - stream.Position} bytes left to be read after reading {DisplayName} data.");
	}

	public void Write(Stream stream)
		=> Write(stream, DefaultEncoding);

	public void Write(Stream stream, Encoding encoding)
	{
		using var writer = new BinaryWriter(stream, encoding, true);

		Write(writer);
	}
}