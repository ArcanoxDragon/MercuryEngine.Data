using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework;

public class HeapManager(ulong startingAddress = 0)
{
	private readonly Dictionary<IBinaryField, ulong> fieldAddresses  = new(ReferenceEqualityComparer.Instance);
	private readonly Dictionary<ulong, IBinaryField> fieldsByAddress = [];
	private readonly List<Allocation>                allocations     = [];
	private readonly Queue<Allocation>               writeQueue      = [];

	private bool isWriting;

	public ulong StartingAddress    { get; private set; } = startingAddress;
	public ulong TotalAllocated     { get; private set; }
	public ulong HighestReadAddress { get; private set; }

	/// <summary>
	/// The byte value used when writing padding bytes between allocated blocks of data.
	/// </summary>
	public byte PaddingByte { get; set; }

	public DataMapper? DataMapper { get; set; }

	/// <summary>
	/// Clears all allocated fields and empties the heap.
	/// </summary>
	public void Reset()
	{
		CheckState();
		this.fieldAddresses.Clear();
		this.fieldsByAddress.Clear();
		this.allocations.Clear();
		TotalAllocated = 0;
	}

	/// <summary>
	/// Clears all allocated fields, empties the heap, and sets the starting address to the provided value.
	/// </summary>
	public void Reset(ulong startingAddress)
	{
		Reset();
		StartingAddress = startingAddress;
	}

	/// <summary>
	/// If an address has already been allocated for the provided <paramref name="field"/>, this method
	/// returns that address. Otherwise, it allocates space on the heap and returns the new address.
	/// </summary>
	public ulong GetAddressOrAllocate(IBinaryField field, uint startByteAlignment = 0, uint endByteAlignment = 0, string? description = null)
	{
		if (this.fieldAddresses.TryGetValue(field, out var address))
			return address;

		return Allocate(field, startByteAlignment, endByteAlignment, description);
	}

	/// <summary>
	/// Allocates a chunk of data on the heap large enough to hold the provided <paramref name="field"/>
	/// and returns the address (relative to the start of the file, when saved).
	/// </summary>
	public ulong Allocate(IBinaryField field, uint startByteAlignment = 0, uint endByteAlignment = 0, string? description = null)
	{
		if (this.fieldAddresses.ContainsKey(field))
			throw new InvalidOperationException("Space has already been allocated for the provided field");

		var address = StartingAddress + TotalAllocated;
		var sizeWithPadding = 0u;

		if (startByteAlignment > 0)
		{
			var prePaddingNeeded = MathHelper.GetNeededPaddingForAlignment(address, startByteAlignment);

			address += prePaddingNeeded;
			sizeWithPadding += prePaddingNeeded;
		}

		var fieldSize = field.GetSize((uint) address);

		sizeWithPadding += fieldSize;

		if (endByteAlignment > 0)
		{
			var postPaddingNeeded = MathHelper.GetNeededPaddingForAlignment(address + fieldSize, endByteAlignment);

			sizeWithPadding += postPaddingNeeded;
		}

		var allocation = new Allocation(address, field, endByteAlignment, description);

		this.fieldAddresses.Add(field, address);
		this.fieldsByAddress[address] = field;
		this.allocations.Add(allocation);
		TotalAllocated += sizeWithPadding;

		if (this.isWriting)
			this.writeQueue.Enqueue(allocation);

		return address;
	}

	/// <summary>
	/// Registers the provided <paramref name="field"/> instance as the value for the provided
	/// <paramref name="address"/>. Future references to the same address can retrieve this
	/// same instance for later.
	/// </summary>
	public void Register(ulong address, IBinaryField field, uint endByteAlignment = 0)
	{
		this.fieldAddresses[field] = address;
		this.fieldsByAddress[address] = field;

		var allocationSize = field.GetSize((uint) address);
		var endAddress = address + allocationSize;

		if (endByteAlignment > 0)
		{
			var paddingNeeded = MathHelper.GetNeededPaddingForAlignment(endAddress, endByteAlignment);

			allocationSize += paddingNeeded;
			endAddress += paddingNeeded;
		}

		HighestReadAddress = Math.Max(HighestReadAddress, endAddress);
	}

	/// <summary>
	/// Gets the address of the provided <paramref name="field"/>, if it has been allocated one.
	/// </summary>
	public ulong GetAddress(IBinaryField field)
		=> this.fieldAddresses[field];

	/// <summary>
	/// Attempts to get the address of the provided <paramref name="field"/>, if it has been allocated one.
	/// </summary>
	public bool TryGetAddress(IBinaryField field, out ulong address)
		=> this.fieldAddresses.TryGetValue(field, out address);

	/// <summary>
	/// Gets the field instance at the provided <paramref name="address"/>, if the address has been registered.
	/// </summary>
	public IBinaryField? GetField(ulong address)
	{
		if (address == 0)
			return null;

		return this.fieldsByAddress[address];
	}

	/// <summary>
	/// Attempts to get the field instance at the provided <paramref name="address"/>, if the address has been registered.
	/// </summary>
	public bool TryGetField(ulong address, [NotNullWhen(true)] out IBinaryField? field)
	{
		if (address == 0)
		{
			field = null;
			return false;
		}

		return this.fieldsByAddress.TryGetValue(address, out field);
	}

	public void WriteAllocatedFields(BinaryWriter writer, WriteContext context)
	{
		PrepareWriteQueue();

		try
		{
			this.isWriting = true;
			DataMapper.PushRange("Heap", writer);

			IBinaryField? priorField = null;

			while (this.writeQueue.TryDequeue(out var item))
			{
				var (address, field, endByteAlignment, description) = item;
				var currentAddress = (ulong) writer.BaseStream.Position;

				if (address < currentAddress)
					throw new IOException($"Tried to write field \"{field}\" allocated to address 0x{address:X16}, but prior field \"{priorField}\" wrote past that address!");

				if (currentAddress < address)
				{
					// Write padding bytes until we get to the desired address
					for (var i = currentAddress; i < address; i++)
						writer.Write(PaddingByte);
				}

				try
				{
					DataMapper.PushRange($"Heap allocation at 0x{address:X16}: {description ?? field.GetType().GetDisplayName()}", writer);
					field.WriteWithDataMapper(writer, DataMapper, context);
				}
				finally
				{
					DataMapper.PopRange(writer);
				}

				if (endByteAlignment > 0)
				{
					var paddingNeeded = writer.BaseStream.GetNeededPaddingForAlignment(endByteAlignment);

					for (var i = 0; i < paddingNeeded; i++)
						writer.Write(PaddingByte);
				}

				priorField = field;
			}
		}
		finally
		{
			DataMapper.PopRange(writer);
			this.isWriting = false;
			this.writeQueue.Clear();
		}
	}

	public async Task WriteAllocatedFieldsAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		PrepareWriteQueue();

		try
		{
			this.isWriting = true;
			await DataMapper.PushRangeAsync("Heap", writer, cancellationToken).ConfigureAwait(false);

			var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);

			while (this.writeQueue.TryDequeue(out var item))
			{
				var (address, field, endByteAlignment, description) = item;
				var currentAddress = (ulong) baseStream.Position;

				if (address < currentAddress)
					throw new IOException($"Tried to write a field allocated to address 0x{address:X16}, but the prior field wrote past that address!");

				if (currentAddress < address)
				{
					// Write padding bytes until we get to the desired address
					for (var i = currentAddress; i < address; i++)
						await writer.WriteAsync(PaddingByte, cancellationToken).ConfigureAwait(false);
				}

				try
				{
					await DataMapper.PushRangeAsync($"Heap allocation at 0x{address:X16}: {description ?? field.GetType().GetDisplayName()}", writer, cancellationToken).ConfigureAwait(false);
					await field.WriteWithDataMapperAsync(writer, DataMapper, context, cancellationToken).ConfigureAwait(false);
					await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
				}
				finally
				{
					await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
				}

				if (endByteAlignment > 0)
				{
					var paddingNeeded = baseStream.GetNeededPaddingForAlignment(endByteAlignment);

					for (var i = 0; i < paddingNeeded; i++)
						await writer.WriteAsync(PaddingByte, cancellationToken).ConfigureAwait(false);

					if (paddingNeeded > 0)
						await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
				}
			}

			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			this.isWriting = false;
			this.writeQueue.Clear();
		}
	}

	private void PrepareWriteQueue()
	{
		this.writeQueue.Clear();

		foreach (var allocation in this.allocations)
			this.writeQueue.Enqueue(allocation);
	}

	private void CheckState([CallerMemberName] string? callerName = null)
	{
		if (this.isWriting)
			throw new InvalidOperationException($"{callerName} cannot be called while writing allocated data");
	}

	private readonly record struct Allocation(ulong Address, IBinaryField Field, uint EndByteAlignment, string? Description);
}