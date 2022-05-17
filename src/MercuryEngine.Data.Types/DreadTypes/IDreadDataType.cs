using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Types.DreadTypes;

/// <summary>
/// Base interface for automatically generated Dread data types to implement.
/// </summary>
[PublicAPI]
public interface IDreadDataType : IBinaryDataType
{
	/// <summary>
	/// The original name of the type as it exists in the Dread game code.
	/// </summary>
	string TypeName { get; }
}