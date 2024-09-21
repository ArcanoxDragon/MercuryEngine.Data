namespace MercuryEngine.Data.Core.Utility;

internal sealed class EmptyDisposable : IDisposable
{
	public static EmptyDisposable Instance { get; } = new();

	private EmptyDisposable() { }

	public void Dispose() { }
}