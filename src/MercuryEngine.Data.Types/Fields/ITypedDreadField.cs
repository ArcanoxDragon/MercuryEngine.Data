using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Types.Fields;

/// <summary>
/// Base interface for automatically generated Dread data types to implement.
/// </summary>
[PublicAPI]
public interface ITypedDreadField : IBinaryField
{
	/// <summary>
	/// The original name of the type as it exists in the Dread game code.
	/// </summary>
	string TypeName { get; }

	/// <summary>
	/// The CRC64 hash of the type name.
	/// </summary>
	ulong TypeId => TypeName.GetCrc64();
}