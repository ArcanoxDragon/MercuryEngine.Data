using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// A <see cref="IFieldHandler"/> that reads/writes directly from/to a <see cref="IBinaryField{T}"/>
/// without having an associated property on a data structure. Can be used for constant/"magic"
/// headers, padding, etc.
/// </summary>
public sealed class ConstantValueFieldHandler<T>(IBinaryField<T> field, bool assertValueOnRead = true) : IFieldHandler
where T : notnull
{
	private readonly T expectedValue = field.Value;

	public uint Size => field.Size;

	public bool HasMeaningfulData => Field.HasMeaningfulData;

	public IBinaryField Field => field;

	public void Reset() => field.Reset();

	public void HandleRead(BinaryReader reader)
	{
		field.Read(reader);

		if (assertValueOnRead)
			AssertValue();
	}

	public void HandleWrite(BinaryWriter writer) => field.Write(writer);

	public async Task HandleReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

		if (assertValueOnRead)
			AssertValue();
	}

	public Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> field.WriteAsync(writer, cancellationToken);

	private void AssertValue()
	{
		if (!Equals(field.Value, this.expectedValue))
			throw new IOException($"Expected constant value \"{this.expectedValue}\" but found \"{field.Value}\" instead");
	}
}