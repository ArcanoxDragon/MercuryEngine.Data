namespace MercuryEngine.Data.Core.Framework.IO;

public sealed class WriteContext(HeapManager heapManager)
{
	public HeapManager HeapManager { get; } = heapManager;
}