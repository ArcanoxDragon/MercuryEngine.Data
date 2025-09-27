using System.Diagnostics.CodeAnalysis;
using System.Text;
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

	public void Read(Stream stream)
		=> Read(stream, DefaultEncoding);

	public void Read(Stream stream, Encoding encoding)
	{
		using var reader = new BinaryReader(stream, encoding, true);
		var heapManager = new HeapManager { DataMapper = DataMapper };
		var context = new ReadContext(heapManager, this);

		base.Read(reader, context);

		ValidateReadSize(stream, context);
	}

	public void Write(Stream stream)
		=> Write(stream, DefaultEncoding);

	public void Write(Stream stream, Encoding encoding)
	{
		using var writer = new BinaryWriter(stream, encoding, true);
		var heapManager = new HeapManager { DataMapper = DataMapper };
		var context = new WriteContext(heapManager, this);

		DataMapper?.Reset();
		base.Write(writer, context);
	}

	public Task ReadAsync(Stream stream, CancellationToken cancellationToken = default)
		=> ReadAsync(stream, DefaultEncoding, cancellationToken);

	public async Task ReadAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, encoding, true);
		var heapManager = new HeapManager { DataMapper = DataMapper };
		var context = new ReadContext(heapManager, this);

		await ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);

		ValidateReadSize(stream, context);
	}

	public Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
		=> WriteAsync(stream, DefaultEncoding, cancellationToken);

	public async Task WriteAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var writer = new AsyncBinaryWriter(stream, encoding, true);
		var heapManager = new HeapManager { DataMapper = DataMapper };
		var context = new WriteContext(heapManager, this);

		DataMapper?.Reset();
		await base.WriteAsync(writer, context, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	#region Heap-related overrides

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		if (ReferenceEquals(context.Root, this))
		{
			// Reset the heap manager, and set its starting address to the first byte after the structure's own fields
			var nonHeapSize = GetSize(0u);

			context.HeapManager.Reset(nonHeapSize);
		}
	}

	protected override void WriteCore(BinaryWriter writer, WriteContext context)
	{
		base.WriteCore(writer, context);

		if (context.HeapManager.TotalAllocated > 0)
			context.HeapManager.WriteAllocatedFields(writer, context);
	}

	protected override async Task WriteAsyncCore(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await base.WriteAsyncCore(writer, context, cancellationToken).ConfigureAwait(false);

		if (context.HeapManager.TotalAllocated > 0)
			await context.HeapManager.WriteAllocatedFieldsAsync(writer, context, cancellationToken).ConfigureAwait(false);
	}

	#endregion

	private void ValidateReadSize(Stream stream, ReadContext context)
	{
		var highestReadAddress = Math.Max((ulong) stream.Position, context.HeapManager.HighestReadAddress);
		var endAddress = (ulong) stream.Length;

		if (highestReadAddress < endAddress)
			throw new IOException($"There were {endAddress - highestReadAddress} bytes left to be read after reading {DisplayName} data.");
	}
}