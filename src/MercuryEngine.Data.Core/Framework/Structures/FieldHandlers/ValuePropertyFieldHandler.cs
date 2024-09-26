using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a property of type <typeparamref name="T"/> using a <see cref="IBinaryField{T}"/> with
/// a <see cref="IBinaryField{T}.Value"/> of type <typeparamref name="T"/>.
/// </summary>
public class ValuePropertyFieldHandler<T>(IBinaryField<T> field, PropertyInfo property, bool nullable = false) : IFieldHandler
where T : notnull
{
	private readonly Func<object, T?>  getter = ReflectionUtility.GetGetter<T?>(property);
	private readonly Action<object, T> setter = ReflectionUtility.GetSetter<T>(property);

	public uint GetSize(IDataStructure dataStructure)
		=> PrepareForWrite(dataStructure) ? field.Size : 0;

	public IBinaryField GetField(IDataStructure dataStructure)
		=> field;

	public void Reset(IDataStructure dataStructure) => field.Reset();

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader)
	{
		field.Read(reader);
		this.setter(dataStructure, field.Value);
	}

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer)
	{
		if (!PrepareForWrite(dataStructure))
			return;

		field.Write(writer);
	}

	public async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		this.setter(dataStructure, field.Value);
	}

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		if (!PrepareForWrite(dataStructure))
			return Task.CompletedTask;

		return field.WriteAsync(writer, cancellationToken);
	}

	private bool PrepareForWrite(IDataStructure dataStructure)
	{
		var value = this.getter(dataStructure);

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