using JetBrains.Annotations;

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
	/// Gets the size of this <see cref="IDataStructureField"/> when stored in a binary format.
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
}