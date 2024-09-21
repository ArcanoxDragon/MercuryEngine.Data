using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a nullable property that can hold a direct <see cref="IBinaryField"/> instance.
/// </summary>
public class NullableDirectPropertyFieldHandler(object owner, PropertyInfo property, Func<IBinaryField> fieldFactory) : IFieldHandler
{
	public uint Size              => Field?.Size ?? 0u;
	public bool HasMeaningfulData => Field is { HasMeaningfulData: true };

	public IBinaryField? Field
	{
		get => property.GetValue(owner) as IBinaryField;
		set => property.SetValue(owner, value);
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