using System.Text;
using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Core.Framework;

public abstract class BinaryFormat<T> : DataStructure<T>
where T : BinaryFormat<T>
{
	/// <summary>
	/// Gets the display name of this <see cref="BinaryFormat{T}"/>.
	/// </summary>
	public abstract string DisplayName { get; }

	public void Read(Stream stream)
	{
		using var reader = new BinaryReader(stream, Encoding.UTF8, true);

		Read(reader);

		if (stream.Position != stream.Length)
			throw new IOException($"There were {stream.Length - stream.Position} bytes left to be read after reading {DisplayName} data.");
	}

	public void Write(Stream stream)
	{
		using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

		Write(writer);
	}
}