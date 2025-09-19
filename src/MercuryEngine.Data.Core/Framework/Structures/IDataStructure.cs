using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures;

[PublicAPI]
public interface IDataStructure : IBinaryField
{
	/// <summary>
	/// Gets a dictionary that can be used to store additional backing field instances that
	/// are associated with this <see cref="IDataStructure"/>, but that are not necessary to
	/// expose publicly to consumers other than the owner of the backing field.
	/// </summary>
	IDictionary<Guid, IBinaryField> BackingFields { get; }
}