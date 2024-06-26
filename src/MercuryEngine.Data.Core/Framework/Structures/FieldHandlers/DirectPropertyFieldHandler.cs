using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a property that holds a direct <see cref="IBinaryField"/> instance.
/// </summary>
public class DirectPropertyFieldHandler(object owner, PropertyInfo property) : IFieldHandler
{
	public uint Size => Field.Size;

	public IBinaryField Field
		=> ( property.GetValue(owner) as IBinaryField )
		   ?? throw new InvalidOperationException($"Property \"{property.Name}\" was null while reading or writing a field " +
												  $"on {owner.GetType().FullName}. {nameof(IBinaryField)} properties must " +
												  $"never have a null value.");

	public void Reset()
		=> ( Field as IResettableField )?.Reset();

	public void HandleRead(BinaryReader reader)
		=> Field.Read(reader);

	public void HandleWrite(BinaryWriter writer)
		=> Field.Write(writer);

	public Task HandleReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
		=> Field.ReadAsync(reader, cancellationToken);

	public Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> Field.WriteAsync(writer, cancellationToken);
}