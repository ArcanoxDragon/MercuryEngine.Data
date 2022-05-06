using JetBrains.Annotations;
using MercuryEngine.Data.Extensions;

namespace MercuryEngine.Data.Framework.Components;

[PublicAPI]
public interface IBinaryComponent
{
	/// <summary>
	/// Gets whether or not this <see cref="IBinaryComponent"/> represents a data type or structure that has a
	/// predetermined or fixed size.
	/// </summary>
	bool IsFixedSize { get; }

	/// <summary>
	/// Returns whether or not this component can successfully read from the provided <paramref name="stream"/>.
	/// </summary>
	bool Validate(Stream stream);

	/// <summary>
	/// Reads data for this component from the provided <paramref name="reader"/> and returns it.
	/// </summary>
	object Read(BinaryReader reader);

	/// <summary>
	/// Writes the given <paramref name="data"/> for this component into the provided <paramref name="writer"/>.
	/// </summary>
	void Write(BinaryWriter writer, object data);
}

[PublicAPI]
public interface IBinaryComponent<T> : IBinaryComponent
where T : notnull
{
	/// <summary>
	/// Reads data for this component from the provided <paramref name="reader"/> and returns it.
	/// </summary>
	new T Read(BinaryReader reader);

	/// <summary>
	/// Writes the given <paramref name="data"/> for this component into the provided <paramref name="writer"/>.
	/// </summary>
	void Write(BinaryWriter writer, T data);
}

[PublicAPI]
public interface IFixedSizeBinaryComponent : IBinaryComponent
{
	/// <summary>
	/// Gets the size of this <see cref="IFixedSizeBinaryComponent"/>.
	/// </summary>
	uint Size { get; }

	bool IBinaryComponent.IsFixedSize => true;
	bool IBinaryComponent.Validate(Stream stream) => stream.HasBytes(Size);
}