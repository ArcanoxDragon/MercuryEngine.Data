using JetBrains.Annotations;

namespace MercuryEngine.Data.Framework;

[PublicAPI]
public interface IBinaryFormat
{
	/// <summary>
	/// Reads data from the provided <paramref name="stream"/> in the format described by this <see cref="IBinaryFormat"/>.
	/// </summary>
	void Read(Stream stream);

	/// <summary>
	/// Writes data to the provided <paramref name="stream"/> in the format described by this <see cref="IBinaryFormat"/>.
	/// </summary>
	void Write(Stream stream);
}