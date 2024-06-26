using System.Text;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Structures;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework;

[PublicAPI]
public abstract class BinaryFormat<T> : DataStructure<T>
where T : BinaryFormat<T>, new()
{
	#region Static Factory

	// TODO: Static extensions in C# 13

	/// <summary>
	/// Returns a new instance of <typeparamref name="T"/> that has been loaded from the provided <paramref name="stream"/>.
	/// </summary>
	public static T From(Stream stream)
	{
		var format = new T();

		format.Read(stream);

		return format;
	}

	/// <summary>
	/// Returns a new instance of <typeparamref name="T"/> that has been loaded from the provided <paramref name="stream"/>
	/// using the provided <paramref name="encoding"/>.
	/// </summary>
	public static T From(Stream stream, Encoding encoding)
	{
		var format = new T();

		format.Read(stream, encoding);

		return format;
	}

	/// <summary>
	/// Returns a new instance of <typeparamref name="T"/> that has been loaded from the provided <paramref name="stream"/>.
	/// </summary>
	public static async Task<T> FromAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		var format = new T();

		await format.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

		return format;
	}

	/// <summary>
	/// Returns a new instance of <typeparamref name="T"/> that has been loaded from the provided <paramref name="stream"/>
	/// using the provided <paramref name="encoding"/>.
	/// </summary>
	public static async Task<T> FromAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		var format = new T();

		await format.ReadAsync(stream, encoding, cancellationToken).ConfigureAwait(false);

		return format;
	}

	#endregion

	private static readonly Encoding DefaultEncoding = new UTF8Encoding();

	/// <summary>
	/// Gets the display name of this <see cref="BinaryFormat{T}"/>.
	/// </summary>
	public abstract string DisplayName { get; }

	public void Read(Stream stream)
		=> Read(stream, DefaultEncoding);

	public void Read(Stream stream, Encoding encoding)
	{
		using var reader = new BinaryReader(stream, encoding, true);

		Read(reader);

		if (stream.Position != stream.Length)
			throw new IOException($"There were {stream.Length - stream.Position} bytes left to be read after reading {DisplayName} data.");
	}

	public void Write(Stream stream)
		=> Write(stream, DefaultEncoding);

	public void Write(Stream stream, Encoding encoding)
	{
		using var writer = new BinaryWriter(stream, encoding, true);

		DataMapper?.Reset();
		Write(writer);
	}

	public Task ReadAsync(Stream stream, CancellationToken cancellationToken = default)
		=> ReadAsync(stream, DefaultEncoding, cancellationToken);

	public async Task ReadAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, encoding, true);

		await ReadAsync(reader, cancellationToken).ConfigureAwait(false);

		if (stream.Position != stream.Length)
			throw new IOException($"There were {stream.Length - stream.Position} bytes left to be read after reading {DisplayName} data.");
	}

	public Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
		=> WriteAsync(stream, DefaultEncoding, cancellationToken);

	public async Task WriteAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var writer = new AsyncBinaryWriter(stream, encoding, true);

		DataMapper?.Reset();
		await WriteAsync(writer, cancellationToken).ConfigureAwait(false);
	}
}