using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Core.Framework.IO;

public sealed class ReadContext(IDataStructure root, HeapManager heapManager)
{
	public IDataStructure Root        { get; } = root;
	public HeapManager    HeapManager { get; } = heapManager;
}