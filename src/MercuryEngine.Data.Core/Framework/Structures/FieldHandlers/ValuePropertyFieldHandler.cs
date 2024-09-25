using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a property of type <typeparamref name="T"/> using a <see cref="IBinaryField{T}"/> with
/// a <see cref="IBinaryField{T}.Value"/> of type <typeparamref name="T"/>.
/// </summary>
public class ValuePropertyFieldHandler<T>(IBinaryField<T> field, object owner, PropertyInfo property, bool nullable = false) : IFieldHandler
where T : notnull
{
	private readonly Func<object, T?>  getter = ReflectionUtility.GetGetter<T?>(property);
	private readonly Action<object, T> setter = ReflectionUtility.GetSetter<T>(property);

	public uint Size              => PrepareForWrite() ? field.Size : 0;

	public IBinaryField Field => field;

	public void Reset() => field.Reset();

	public void HandleRead(BinaryReader reader)
	{
		field.Read(reader);
		this.setter(owner, field.Value);
	}

	public void HandleWrite(BinaryWriter writer)
	{
		if (!PrepareForWrite())
			return;

		field.Write(writer);
	}

	public async Task HandleReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		this.setter(owner, field.Value);
	}

	public Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		if (!PrepareForWrite())
			return Task.CompletedTask;

		return field.WriteAsync(writer, cancellationToken);
	}

	private bool PrepareForWrite()
	{
		var value = this.getter(owner);

		if (value is null)
		{
			if (nullable)
				return false;

			throw new InvalidOperationException($"Property \"{property.Name}\" on {owner.GetType().FullName} returned null while writing to a binary field");
		}

		field.Value = value;
		return true;
	}
}