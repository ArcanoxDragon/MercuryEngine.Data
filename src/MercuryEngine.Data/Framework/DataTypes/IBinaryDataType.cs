using JetBrains.Annotations;
using Newtonsoft.Json;

namespace MercuryEngine.Data.Framework.DataTypes;

/// <summary>
/// Represents a type of data that can be written to or read from a binary format.
/// </summary>
[PublicAPI]
public interface IBinaryDataType
{
	/// <summary>
	/// Gets the size of this <see cref="IBinaryDataType"/> (how many bytes it will take up when written as binary data).
	/// </summary>
	[JsonIgnore]
	uint Size { get; }

	/// <summary>
	/// Reads data for this data type from the provided <paramref name="reader"/>.
	/// </summary>
	void Read(BinaryReader reader);

	/// <summary>
	/// Writes data for this data type into the provided <paramref name="writer"/>.
	/// </summary>
	void Write(BinaryWriter writer);
}

/// <summary>
/// Represents a type of data that can be written to or read from a binary format,
/// and that is represented by the <typeparamref name="T"/> type in managed code.
/// </summary>
public interface IBinaryDataType<T> : IBinaryDataType
where T : notnull
{
	/// <summary>
	/// Gets or sets the managed value of this data type.
	/// </summary>
	T Value { get; set; }
}