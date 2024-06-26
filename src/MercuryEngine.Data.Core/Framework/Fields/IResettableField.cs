using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.Fields;

/// <summary>
/// Represents a <see cref="IBinaryField"/> that can be "reset" to a default state.
/// </summary>
[PublicAPI]
public interface IResettableField : IBinaryField
{
	void Reset();
}