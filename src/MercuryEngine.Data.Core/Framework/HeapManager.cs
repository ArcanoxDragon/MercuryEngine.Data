using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework;

public class HeapManager(ulong startingAddress = 0)
{
	private readonly Dictionary<IBinaryField, ulong> fieldAddresses  = [];
	private readonly Dictionary<ulong, IBinaryField> fieldsByAddress = [];
	private readonly List<IBinaryField>              allocatedFields = [];

	private bool isWriting;

	public ulong StartingAddress { get; private set; } = startingAddress;
	public ulong TotalAllocated  { get; private set; }

	public DataMapper? DataMapper { get; set; }

	/// <summary>
	/// Clears all allocated fields and empties the heap.
	/// </summary>
	public void Reset()
	{
		CheckState();
		this.fieldAddresses.Clear();
		this.fieldsByAddress.Clear();
		this.allocatedFields.Clear();
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
	public ulong GetAddressOrAllocate(IBinaryField field)
	{
		if (this.fieldAddresses.TryGetValue(field, out var address))
			return address;

		return Allocate(field);
	}

	/// <summary>
	/// Allocates a chunk of data on the heap large enough to hold the provided <paramref name="field"/>
	/// and returns the address (relative to the start of the file, when saved).
	/// </summary>
	public ulong Allocate(IBinaryField field)
	{
		CheckState();

		if (this.fieldAddresses.ContainsKey(field))
			throw new InvalidOperationException("Space has already been allocated for the provided field");

		var address = StartingAddress + TotalAllocated;
		var size = field.Size;

		this.fieldAddresses.Add(field, address);
		this.fieldsByAddress[address] = field;
		this.allocatedFields.Add(field);
		TotalAllocated += size;

		return address;
	}

	/// <summary>
	/// Registers the provided <paramref name="field"/> instance as the value for the provided
	/// <paramref name="address"/>. Future references to the same address can retrieve this
	/// same instance for later.
	/// </summary>
	public void Register(ulong address, IBinaryField field)
	{
		this.fieldAddresses[field] = address;
		this.fieldsByAddress[address] = field;
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
		try
		{
			this.isWriting = true;
			DataMapper.PushRange("Heap", writer);

			foreach (var field in this.allocatedFields)
				field.WriteWithDataMapper(writer, DataMapper, context);

			DataMapper.PopRange(writer);
		}
		finally
		{
			this.isWriting = false;
		}
	}

	public async Task WriteAllocatedFieldsAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		try
		{
			this.isWriting = true;
			await DataMapper.PushRangeAsync("Heap", writer, cancellationToken).ConfigureAwait(false);

			foreach (var field in this.allocatedFields)
				await field.WriteWithDataMapperAsync(writer, DataMapper, context, cancellationToken).ConfigureAwait(false);

			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			this.isWriting = false;
		}
	}

	private void CheckState([CallerMemberName] string? callerName = null)
	{
		if (this.isWriting)
			throw new InvalidOperationException($"{callerName} cannot be called while writing allocated data");
	}
}