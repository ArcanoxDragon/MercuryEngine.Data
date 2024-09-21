using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a <see cref="IBinaryField"/> instance without it being bound to a propery.
/// </summary>
public class InlineFieldHandler(IBinaryField field) : IFieldHandler
{
	public uint Size              => Field.Size;
	public bool HasMeaningfulData => Field.HasMeaningfulData;

	public IBinaryField Field { get; } = field;

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