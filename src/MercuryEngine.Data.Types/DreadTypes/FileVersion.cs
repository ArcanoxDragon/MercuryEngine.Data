using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.DreadTypes;

public sealed class FileVersion : IBinaryField, IEquatable<FileVersion>
{
	public FileVersion() { }

	public FileVersion(ushort major, byte minor, byte patch)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
	}

	public ushort Major { get; set; }
	public byte   Minor { get; set; }
	public byte   Patch { get; set; }

	public uint Size => sizeof(ushort) + 2 * sizeof(byte);

	public void Read(BinaryReader reader)
	{
		Major = reader.ReadUInt16();
		Minor = reader.ReadByte();
		Patch = reader.ReadByte();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Major);
		writer.Write(Minor);
		writer.Write(Patch);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Major = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
		Minor = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
		Patch = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(Major, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Minor, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Patch, cancellationToken).ConfigureAwait(false);
	}

	public override string ToString() => $"{Major}.{Minor}.{Patch}";

	#region IEquatable

	public bool Equals(FileVersion? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;

		return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
	}

	public override bool Equals(object? obj)
		=> ReferenceEquals(this, obj) || obj is FileVersion other && Equals(other);

	public override int GetHashCode()
		=> HashCode.Combine(Major, Minor, Patch);

	public static bool operator ==(FileVersion? left, FileVersion? right)
		=> Equals(left, right);

	public static bool operator !=(FileVersion? left, FileVersion? right)
		=> !Equals(left, right);

	#endregion
}