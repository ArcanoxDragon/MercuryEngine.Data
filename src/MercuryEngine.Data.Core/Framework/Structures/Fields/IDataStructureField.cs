using JetBrains.Annotations;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// Represents a field on an <see cref="IDataStructure"/>.
/// </summary>
[PublicAPI]
public interface IDataStructureField
{
	/// <summary>
	/// Gets a brief friendly description of the field to use in error messages, etc.
	/// </summary>
	string FriendlyDescription { get; }
}

[PublicAPI]
public interface IDataStructureField<in T> : IDataStructureField
where T : IDataStructure
{
	/// <summary>
	/// Clears data for the field, if possible, so that it may be omitted when writing to a binary format.
	/// </summary>
	void ClearData(T structure);

	/// <summary>
	/// Gets whether or not the field currently has data to be written for the provided <paramref name="structure"/>.
	/// </summary>
	bool HasData(T structure);

	/// <summary>
	/// Gets the size of the field when stored in a binary format.
	/// </summary>
	uint GetSize(T structure);

	/// <summary>
	/// Reads the field for the provided <paramref name="structure"/> from the provided <paramref name="reader"/>.
	/// </summary>
	void Read(T structure, BinaryReader reader);

	/// <summary>
	/// Writes the field for the provided <paramref name="structure"/> to the provided <paramref name="writer"/>.
	/// </summary>
	void Write(T structure, BinaryWriter writer);

	/// <summary>
	/// Asynchronously reads the field for the provided <paramref name="structure"/> from the provided <paramref name="reader"/>.
	/// </summary>
	Task ReadAsync(T structure, AsyncBinaryReader reader, CancellationToken cancellationToken);

	/// <summary>
	/// Asynchronously writes the field for the provided <paramref name="structure"/> to the provided <paramref name="writer"/>.
	/// </summary>
	Task WriteAsync(T structure, AsyncBinaryWriter writer, CancellationToken cancellationToken);
}