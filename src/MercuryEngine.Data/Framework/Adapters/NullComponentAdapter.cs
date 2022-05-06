using JetBrains.Annotations;
using MercuryEngine.Data.Framework.Components;

namespace MercuryEngine.Data.Framework.Adapters;

/// <summary>
/// An <see cref="IComponentAdapter"/> that discards any data read, and that always passes an empty byte array when writing to its component.
/// </summary>
[PublicAPI]
public class NullComponentAdapter : IComponentAdapter
{
	public NullComponentAdapter(IBinaryComponent component)
	{
		Component = component;
	}

	public IBinaryComponent Component { get; }

	public void Read(BinaryReader reader)
		=> Component.Read(reader);

	public void Write(BinaryWriter writer)
		=> Component.Write(writer, Array.Empty<byte>());
}