using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a nullable property that can hold a direct <see cref="IBinaryField"/> instance.
/// </summary>
public class NullableDirectPropertyFieldHandler<TField>(object owner, PropertyInfo property, Func<TField> fieldFactory) : IFieldHandler
where TField : IBinaryField
{
	private readonly Func<object, TField?>   getter = ReflectionUtility.GetGetter<TField?>(property);
	private readonly Action<object, TField?> setter = ReflectionUtility.GetSetter<TField?>(property);

	public uint Size              => Field?.Size ?? 0u;
	public bool HasMeaningfulData => Field is { HasMeaningfulData: true };

	public IBinaryField? Field
	{
		get => this.getter(owner);
		set => this.setter(owner, (TField?) value);
	}

	public void Reset() => Field = null;

	public void HandleRead(BinaryReader reader)
	{
		Field ??= fieldFactory();
		Field.Read(reader);
	}

	public void HandleWrite(BinaryWriter writer)
		=> Field?.Write(writer);

	public Task HandleReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		Field ??= fieldFactory();
		return Field.ReadAsync(reader, cancellationToken);
	}

	public Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> Field?.WriteAsync(writer, cancellationToken) ?? Task.CompletedTask;
}