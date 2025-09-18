using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework;

[PublicAPI]
public abstract class BinaryFormat<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	T
> : DataStructure<T>
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

	/// <summary>
	/// Gets the <see cref="Framework.HeapManager"/> instance responsible for managing allocations to heap data within a binary file.
	/// </summary>
	[JsonIgnore]
	public HeapManager HeapManager { get; } = new();

	public void Read(Stream stream)
		=> Read(stream, DefaultEncoding);

	public void Read(Stream stream, Encoding encoding)
	{
		using var reader = new BinaryReader(stream, encoding, true);
		var context = new ReadContext(HeapManager);

		base.Read(reader, context);

		if (stream.Position != stream.Length)
			throw new IOException($"There were {stream.Length - stream.Position} bytes left to be read after reading {DisplayName} data.");
	}

	public void Write(Stream stream)
		=> Write(stream, DefaultEncoding);

	public void Write(Stream stream, Encoding encoding)
	{
		using var writer = new BinaryWriter(stream, encoding, true);
		var context = new WriteContext(HeapManager);

		DataMapper?.Reset();
		base.Write(writer, context);
	}

	public Task ReadAsync(Stream stream, CancellationToken cancellationToken = default)
		=> ReadAsync(stream, DefaultEncoding, cancellationToken);

	public async Task ReadAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, encoding, true);
		var context = new ReadContext(HeapManager);

		await ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);

		if (stream.Position != stream.Length)
			throw new IOException($"There were {stream.Length - stream.Position} bytes left to be read after reading {DisplayName} data.");
	}

	public Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
		=> WriteAsync(stream, DefaultEncoding, cancellationToken);

	public async Task WriteAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var writer = new AsyncBinaryWriter(stream, encoding, true);
		var context = new WriteContext(HeapManager);

		DataMapper?.Reset();
		await base.WriteAsync(writer, context, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	#region Heap-related overrides

	// TODO: It's less than ideal that the "Size" property does not include the size of the heap data.
	//  Is there a way we could make it include that?

	protected override void BeforeWrite()
	{
		base.BeforeWrite();

		// Reset the heap manager, and set its starting address to the first byte after the structure's own fields
		var nonHeapSize = Size;

		HeapManager.Reset(nonHeapSize);
		HeapManager.DataMapper = DataMapper;
	}

	protected override void AfterWrite()
	{
		base.AfterWrite();

		HeapManager.DataMapper = null; // Don't keep a reference longer than we need
	}

	protected override void WriteCore(BinaryWriter writer, WriteContext context)
	{
		base.WriteCore(writer, context);

		if (HeapManager.TotalAllocated > 0)
			HeapManager.WriteAllocatedFields(writer, context);
	}

	protected override async Task WriteAsyncCore(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await base.WriteAsyncCore(writer, context, cancellationToken).ConfigureAwait(false);

		if (HeapManager.TotalAllocated > 0)
			await HeapManager.WriteAllocatedFieldsAsync(writer, context, cancellationToken).ConfigureAwait(false);
	}

	#endregion
}