using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

namespace MercuryEngine.Data.Core.Framework.Structures;

[DebuggerDisplay("{Description}")]
public sealed class DataStructureField(IFieldHandler handler, string description)
{
	public IFieldHandler Handler     { get; } = handler;
	public string        Description { get; } = description;
}