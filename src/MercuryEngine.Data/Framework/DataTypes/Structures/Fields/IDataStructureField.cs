using JetBrains.Annotations;

namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

[PublicAPI]
public interface IDataStructureField : IBinaryDataType
{
	/// <summary>
	/// Gets a brief friendly description of the field to use in error messages, etc.
	/// </summary>
	string FriendlyDescription { get; }
}