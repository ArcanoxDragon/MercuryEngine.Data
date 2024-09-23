using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Utility;

[PublicAPI]
public enum DuplicateKeyHandlingMode
{
	/// <summary>
	/// When using single-item interfaces to interact with a multi-dictionary, gets and sets
	/// will only operate on the highest-index instance of a particular key and ignore all others.
	/// </summary>
	HighestIndexTakesPriority,

	/// <summary>
	/// When using single-item interfaces to interact with a multi-dictionary, gets and sets
	/// will only operate on the lowest-index instance of a particular key and ignore all others.
	/// </summary>
	LowestIndexTakesPriority,

	/// <summary>
	/// When using single-item interfaces to interact with a multi-dictionary, add operations that
	/// would result in a duplicate key being added will remove all other instances before adding
	/// the new value. There will never be more than one value for any given key.
	/// </summary>
	PreventDuplicateKeys,
}