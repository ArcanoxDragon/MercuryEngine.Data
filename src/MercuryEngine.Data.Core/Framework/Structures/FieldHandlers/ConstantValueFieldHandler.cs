using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

public class ConstantValueFieldHandler(Func<IBinaryField> fieldFactory)
	: FieldHandlerWithBackingField<IDataStructure>(_ => fieldFactory())
{
	public override uint GetSize(IDataStructure dataStructure)
		=> GetField(dataStructure).Size;

	public override void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
		=> GetField(dataStructure).Read(reader, context);

	public override void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context)
		=> GetField(dataStructure).Write(writer, context);

	public override async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
		=> await GetField(dataStructure).ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);

	public override Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
		=> GetField(dataStructure).WriteAsync(writer, context, cancellationToken: cancellationToken);
}

/// <summary>
/// A <see cref="IFieldHandler"/> that reads/writes directly from/to a <see cref="IBinaryField{T}"/>
/// without having an associated property on a data structure. Can be used for constant/"magic"
/// headers, padding, etc.
/// </summary>
public sealed class ConstantValueFieldHandler<T>(Func<IBinaryField<T>> fieldFactory, T expectedValue, bool assertValueOnRead = true)
	: ConstantValueFieldHandler(fieldFactory)
where T : notnull
{
	public override void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
	{
		base.HandleRead(dataStructure, reader, context);

		if (assertValueOnRead)
			AssertValue(dataStructure);
	}

	public override async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await base.HandleReadAsync(dataStructure, reader, context, cancellationToken).ConfigureAwait(false);

		if (assertValueOnRead)
			AssertValue(dataStructure);
	}

	private void AssertValue(IDataStructure dataStructure)
	{
		var field = GetField(dataStructure);

		if (field is not IBinaryField<T> typedField)
			throw new IOException($"Expected constant field to be of type {typeof(IBinaryField<T>).GetDisplayName()}, but it was {field.GetType().GetDisplayName()} instead");

		if (!Equals(typedField.Value, expectedValue))
			throw new IOException($"Expected constant value \"{expectedValue}\" but found \"{typedField.Value}\" instead");
	}
}