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
>(IBinaryField<TValue> field, PropertyInfo property, bool nullable = false) : IFieldHandler
where TValue : notnull
{
	private readonly Func<TOwner, TValue?>  getter = ReflectionUtility.GetGetter<TOwner, TValue?>(property);
	private readonly Action<TOwner, TValue> setter = ReflectionUtility.GetSetter<TOwner, TValue>(property);

	public uint GetSize(IDataStructure dataStructure)
		=> PrepareForWrite(dataStructure) ? field.Size : 0;

	public IBinaryField GetField(IDataStructure dataStructure)
		=> field;

	public void Reset(IDataStructure dataStructure) => field.Reset();

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
	{
		field.Read(reader, context);
		this.setter((TOwner) dataStructure, field.Value);
	}

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context)
	{
		if (!PrepareForWrite(dataStructure))
			return;

		field.Write(writer, context);
	}

	public async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await field.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		this.setter((TOwner) dataStructure, field.Value);
	}

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
	{
		if (!PrepareForWrite(dataStructure))
			return Task.CompletedTask;

		return field.WriteAsync(writer, context, cancellationToken: cancellationToken);
	}

	private bool PrepareForWrite(IDataStructure dataStructure)
	{
		var value = this.getter((TOwner) dataStructure);

		if (value is null)
		{
			if (nullable)
				return false;

			throw new InvalidOperationException($"Property \"{property.Name}\" on {dataStructure.GetType().FullName} returned null while writing to a binary field");
		}

		field.Value = value;
		return true;
	}
}