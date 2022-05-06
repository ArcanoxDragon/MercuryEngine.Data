using JetBrains.Annotations;
using MercuryEngine.Data.Framework.Components;

namespace MercuryEngine.Data.Framework.Adapters;

[PublicAPI]
public interface IComponentAdapter
{
	/// <summary>
	/// The component for which this <see cref="IComponentAdapter"/> is responsible for adapting data.
	/// </summary>
	IBinaryComponent Component { get; }

	/// <summary>
	/// Reads data from the provided <paramref name="reader"/>.
	/// </summary>
	void Read(BinaryReader reader);

	/// <summary>
	/// Writes data to the provided <paramref name="writer"/>.
	/// </summary>
	void Write(BinaryWriter writer);
}

public interface IComponentAdapter<out TComponent> : IComponentAdapter
where TComponent : IBinaryComponent
{
	/// <summary>
	/// The component for which this <see cref="IComponentAdapter"/> is responsible for adapting data.
	/// </summary>
	new TComponent Component { get; }

	IBinaryComponent IComponentAdapter.Component => Component;
}