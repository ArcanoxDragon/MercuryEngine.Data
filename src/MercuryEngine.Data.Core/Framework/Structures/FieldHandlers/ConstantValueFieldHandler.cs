using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
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

	public uint GetSize(IDataStructure dataStructure)
		=> field.Size;

	public IBinaryField GetField(IDataStructure dataStructure)
		=> field;

	public void Reset(IDataStructure dataStructure)
		=> field.Reset();

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
	{
		field.Read(reader, context);

		if (assertValueOnRead)
			AssertValue();
	}

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context) => field.Write(writer, context);

	public async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await field.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);

		if (assertValueOnRead)
			AssertValue();
	}

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
		=> field.WriteAsync(writer, context, cancellationToken: cancellationToken);

	private void AssertValue()
	{
		if (!Equals(field.Value, this.expectedValue))
			throw new IOException($"Expected constant value \"{this.expectedValue}\" but found \"{field.Value}\" instead");
	}
}