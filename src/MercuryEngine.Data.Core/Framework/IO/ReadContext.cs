namespace MercuryEngine.Data.Core.Framework.IO;

public sealed class ReadContext(HeapManager heapManager)
{
	public HeapManager HeapManager { get; } = heapManager;
}