using JetBrains.Annotations;
using MercuryEngine.Data.Extensions;

namespace MercuryEngine.Data.Framework.Components;

[PublicAPI]
public abstract class FixedSizeBinaryComponent : BinaryComponent, IFixedSizeBinaryComponent
{
	public sealed override bool IsFixedSize => true;

	public abstract uint Size { get; }

	public override bool Validate(Stream stream)
		=> stream.HasBytes(Size);
}

[PublicAPI]
public abstract class FixedSizeBinaryComponent<T> : BinaryComponent<T>, IFixedSizeBinaryComponent
where T : notnull
{
	public sealed override bool IsFixedSize => true;

	public abstract uint Size { get; }

	public override bool Validate(Stream stream)
		=> stream.HasBytes(Size);
}