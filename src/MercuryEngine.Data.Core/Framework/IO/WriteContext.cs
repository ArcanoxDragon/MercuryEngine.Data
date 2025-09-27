using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Core.Framework.IO;

public sealed class WriteContext(HeapManager heapManager, IDataStructure? root = null)
{
	public HeapManager     HeapManager { get; } = heapManager;
	public IDataStructure? Root        { get; } = root;
}