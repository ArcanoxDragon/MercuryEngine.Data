using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

/// <summary>
/// Represents data that can be written to or read from a binary format.
/// </summary>
[PublicAPI]
public interface IBinaryField
{
	/// <summary>
	/// Gets the size of this <see cref="IBinaryField"/> (how many bytes it will take up when written as binary data).
	/// </summary>
	[JsonIgnore]
	uint Size { get; }

	/// <summary>
	/// Gets whether or not this field represents "meaningful" or "significant" data. For example, length-prefixed
	/// fields may return <see langword="false"/> if the length they write is <c>0</c>, even though the field
	/// will still write some bytes of data (the length).
	/// </summary>
	bool HasMeaningfulData { get; }

	/// <summary>
	/// Loads this <see cref="IBinaryField"/> from the provided <paramref name="reader"/>.
	/// </summary>
	void Read(BinaryReader reader);

	/// <summary>
	/// Stores this <see cref="IBinaryField"/> into the provided <paramref name="writer"/>.
	/// </summary>
	void Write(BinaryWriter writer);

	/// <summary>
	/// Asynchronously loads this <see cref="IBinaryField"/> from the provided <paramref name="reader"/>.
	/// </summary>
	Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously stores this <see cref="IBinaryField"/> into the provided <paramref name="writer"/>.
	/// </summary>
	Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a <see cref="IBinaryField"/> that will load or store a value of type <typeparamref name="T"/> in a binary format.
/// </summary>
public interface IBinaryField<T> : IResettableField
where T : notnull
{
	/// <summary>
	/// Gets or sets the value being represented as binary data.
	/// </summary>
	T Value { get; set; }
}