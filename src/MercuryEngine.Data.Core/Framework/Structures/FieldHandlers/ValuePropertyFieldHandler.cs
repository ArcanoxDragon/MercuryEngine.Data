using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a property of type <typeparamref name="TValue"/> using a <see cref="IBinaryField{T}"/> with
/// a <see cref="IBinaryField{T}.Value"/> of type <typeparamref name="TValue"/>.
/// </summary>
public class ValuePropertyFieldHandler<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TOwner,
	TValue
>(Func<TOwner, IBinaryField<TValue>> fieldFactory, PropertyInfo property, bool nullable = false)
	: FieldHandlerWithBackingField<TOwner, IBinaryField<TValue>>(fieldFactory)
where TOwner : IDataStructure
where TValue : notnull
{
	private readonly Func<TOwner, TValue?>  getter = ReflectionUtility.GetGetter<TOwner, TValue?>(property);
	private readonly Action<TOwner, TValue> setter = ReflectionUtility.GetSetter<TOwner, TValue>(property);

	public override uint GetSize(IDataStructure dataStructure, uint startPosition)
		=> PrepareForWrite(dataStructure, out var field) ? field.GetSize(startPosition) : 0;

	public override void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
	{
		var field = GetField(dataStructure);

		field.Read(reader, context);
		this.setter((TOwner) dataStructure, field.Value);
	}

	public override void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context)
	{
		if (!PrepareForWrite(dataStructure, out var field))
			return;

		field.Write(writer, context);
	}

	public override async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		var field = GetField(dataStructure);

		await field.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		this.setter((TOwner) dataStructure, field.Value);
	}

	public override Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
	{
		if (!PrepareForWrite(dataStructure, out var field))
			return Task.CompletedTask;

		return field.WriteAsync(writer, context, cancellationToken: cancellationToken);
	}

	private bool PrepareForWrite(IDataStructure dataStructure, [NotNullWhen(true)] out IBinaryField<TValue>? field)
	{
		var value = this.getter((TOwner) dataStructure);

		if (value is null)
		{
			if (nullable)
			{
				field = null;
				return false;
			}

			throw new InvalidOperationException($"Property \"{property.Name}\" on {dataStructure.GetType().FullName} returned null while writing to a binary field");
		}

		field = GetField(dataStructure);
		field.Value = value;
		return true;
	}
}