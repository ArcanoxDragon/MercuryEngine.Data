using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

public interface IDynamicStructureField
{
	/// <summary>
	/// Gets a brief friendly description of the field to use in error messages, etc.
	/// </summary>
	string FriendlyDescription { get; }

	/// <summary>
	/// Gets the name of the field.
	/// </summary>
	string FieldName { get; }

	/// <summary>
	/// The size of the field's data when stored in a binary format.
	/// </summary>
	uint Size { get; }

	/// <summary>
	/// Gets whether or not the field currently holds a value.
	/// </summary>
	bool HasValue { get; }

	/// <summary>
	/// Gets or sets the value of the field.
	/// </summary>
	dynamic Value { get; set; }

	/// <summary>
	/// Clones this field for use on another <see cref="DynamicStructure"/>.
	/// </summary>
	/// <remarks>
	/// The value of the field will not be cloned - only the metadata required to read and write the field.
	/// </remarks>
	IDynamicStructureField Clone(DynamicStructure targetStructure);

	/// <summary>
	/// Clears the value of this field so that it will not be written (unless another value is subsequently assigned).
	/// </summary>
	void ClearValue();

	/// <summary>
	/// Reads data into the field from the provided <paramref name="reader"/>.
	/// </summary>
	void Read(BinaryReader reader);

	/// <summary>
	/// Writes the field's data into the provided <paramref name="writer"/>.
	/// </summary>
	void Write(BinaryWriter writer);

	/// <summary>
	/// Asynchronously reads data into the field from the provided <paramref name="reader"/>.
	/// </summary>
	Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken);

	/// <summary>
	/// Asynchronously writes the field's data into the provided <paramref name="writer"/>.
	/// </summary>
	Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken);
}