using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

[PublicAPI]
public abstract class FieldHandlerWithBackingField<TOwner, TField>(Func<TOwner, TField> fieldFactory) : IFieldHandler
where TOwner : IDataStructure
where TField : IBinaryField
{
	protected Guid UniqueId { get; } = Guid.NewGuid();

	public abstract uint GetSize(IDataStructure dataStructure, uint startPosition);

	IBinaryField IFieldHandler.GetField(IDataStructure dataStructure)
		=> GetField(dataStructure);

	public TField GetField(IDataStructure dataStructure)
	{
		if (!dataStructure.BackingFields.TryGetValue(UniqueId, out var backingField) || backingField is not TField field)
		{
			field = fieldFactory((TOwner) dataStructure);
			dataStructure.BackingFields[UniqueId] = field;
		}

		return field;
	}

	public virtual void Reset(IDataStructure dataStructure)
	{
		if (dataStructure.BackingFields.TryGetValue(UniqueId, out var backingField) && backingField is IResettableField resettable)
			resettable.Reset();
	}

	public abstract void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context);
	public abstract void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context);
	public abstract Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken);
	public abstract Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken);
}

[PublicAPI]
public abstract class FieldHandlerWithBackingField<TOwner>(Func<TOwner, IBinaryField> fieldFactory)
	: FieldHandlerWithBackingField<TOwner, IBinaryField>(fieldFactory)
where TOwner : IDataStructure;