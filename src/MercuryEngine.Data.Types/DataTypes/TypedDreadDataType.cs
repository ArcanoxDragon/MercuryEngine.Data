using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Types.DreadTypes;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.DataTypes;

[PublicAPI]
public class TypedDreadDataType : IBinaryDataType
{
	private const ulong NullTypeId = 0UL;

	public TypedDreadDataType() { }

	public TypedDreadDataType(IDreadDataType initialValue)
		: this(initialValue.TypeName, initialValue) { }

	public TypedDreadDataType(string initialTypeName, IBinaryDataType initialValue)
		: this(new TypedDreadValue(initialTypeName, initialValue)) { }

	public TypedDreadDataType(TypedDreadValue initialValue)
	{
		InnerValue = initialValue;
	}

	[JsonIgnore]
	public ulong InnerTypeId => InnerValue?.TypeId ?? 0L;

	[JsonIgnore]
	public IBinaryDataType? InnerData => InnerValue?.Data;

	public uint Size => InnerValue?.Data.Size ?? 0;

	[JsonPropertyName("Value")]
	private TypedDreadValue? InnerValue { get; set; }

	public void Read(BinaryReader reader)
	{
		var typeId = reader.ReadUInt64();

		if (typeId is NullTypeId)
		{
			// Read NULL; clear out inner value and return
			InnerValue = null;
			return;
		}

		InnerValue = DreadTypeRegistry.CreateValueFor(typeId);
		InnerValue.Data.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		if (InnerValue is null)
		{
			// Write an all-zeroes type ID to indicate NULL
			writer.Write(NullTypeId);
			return;
		}

		writer.Write(InnerValue.TypeId);
		InnerValue.Data.Write(writer);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		var typeId = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);

		if (typeId is NullTypeId)
		{
			// Read NULL; clear out inner value and return
			InnerValue = null;
			return;
		}

		InnerValue = DreadTypeRegistry.CreateValueFor(typeId);
		await InnerValue.Data.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		if (InnerValue is null)
		{
			// Write an all-zeroes type ID to indicate NULL
			await writer.WriteAsync(NullTypeId, cancellationToken).ConfigureAwait(false);
			return;
		}

		await writer.WriteAsync(InnerValue.TypeId, cancellationToken).ConfigureAwait(false);
		await InnerValue.Data.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
	}
}