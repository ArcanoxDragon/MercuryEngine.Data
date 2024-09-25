using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.DreadTypes;

public sealed class FileVersion : IBinaryField, IEquatable<FileVersion>
{
	public FileVersion() { }

	public FileVersion(ushort major, ushort minor)
	{
		Major = major;
		Minor = minor;
	}

	public ushort Major { get; set; }
	public ushort Minor { get; set; }

	public uint Size              => 2 * sizeof(ushort);
	public bool HasMeaningfulData => true;

	public void Read(BinaryReader reader)
	{
		Major = reader.ReadUInt16();
		Minor = reader.ReadUInt16();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Major);
		writer.Write(Minor);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Major = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
		Minor = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(Major, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Minor, cancellationToken).ConfigureAwait(false);
	}

	#region IEquatable

	public bool Equals(FileVersion? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;

		return Major == other.Major && Minor == other.Minor;
	}

	public override bool Equals(object? obj)
		=> ReferenceEquals(this, obj) || obj is FileVersion other && Equals(other);

	public override int GetHashCode()
		=> HashCode.Combine(Major, Minor);

	public static bool operator ==(FileVersion? left, FileVersion? right)
		=> Equals(left, right);

	public static bool operator !=(FileVersion? left, FileVersion? right)
		=> !Equals(left, right);

	#endregion
}