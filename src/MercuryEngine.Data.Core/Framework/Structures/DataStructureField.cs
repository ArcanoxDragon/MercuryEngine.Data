using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures;

[DebuggerDisplay("{Description}")]
public sealed class DataStructureField(IFieldHandler handler, string description)
{
	public IFieldHandler Handler     { get; } = handler;
	public string        Description { get; } = description;

	public uint Size => Handler.Size;

	public void Reset() => Handler.Reset();
	public void Read(BinaryReader reader) => Handler.HandleRead(reader);
	public void Write(BinaryWriter writer) => Handler.HandleWrite(writer);

	public Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
		=> Handler.HandleReadAsync(reader, cancellationToken);

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> Handler.HandleWriteAsync(writer, cancellationToken);
}