namespace MercuryEngine.Data.TegraTextureLib.Extensions;

internal static class StreamExtensions
{
	public static SeekToken TemporarySeek(this Stream stream, long newOffset, SeekOrigin origin = SeekOrigin.Begin)
	{
		var originalPosition = stream.Position;
		stream.Seek(newOffset, origin);
		return new SeekToken(stream, originalPosition);
	}

	internal readonly struct SeekToken(Stream stream, long originalPosition) : IDisposable
	{
		public void Dispose()
			=> stream.Seek(originalPosition, SeekOrigin.Begin);
	}
}