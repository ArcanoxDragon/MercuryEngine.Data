using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a property that holds a direct <see cref="IBinaryField"/> instance.
/// </summary>
public class DirectPropertyFieldHandler(object owner, PropertyInfo property) : IFieldHandler
{
	private readonly Func<object, IBinaryField?> getter = ReflectionUtility.GetGetter<IBinaryField?>(property);

	public uint Size              => Field.Size;
	public bool HasMeaningfulData => Field.HasMeaningfulData;

	public IBinaryField Field
		=> this.getter(owner)
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