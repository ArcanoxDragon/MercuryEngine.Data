using System.Text;

namespace MercuryEngine.Data.Core.Framework;

public interface IBinaryFormat
{
	void Read(Stream stream);
	void Read(Stream stream, Encoding encoding);
	void Write(Stream stream);
	void Write(Stream stream, Encoding encoding);
	Task ReadAsync(Stream stream, CancellationToken cancellationToken = default);
	Task ReadAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default);
	Task WriteAsync(Stream stream, CancellationToken cancellationToken = default);
	Task WriteAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default);
}