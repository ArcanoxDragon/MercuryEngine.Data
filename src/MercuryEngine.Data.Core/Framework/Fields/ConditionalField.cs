using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class ConditionalField<TField>(Func<bool> predicate, TField innerField) : IBinaryField
where TField : IBinaryField
{
	private TField innerField = innerField;

	public uint GetSize(uint startPosition) => predicate() ? this.innerField.GetSize(startPosition) : 0;

	public void Read(BinaryReader reader, ReadContext context)
	{
		if (!predicate())
			return;

		this.innerField.Read(reader, context);
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		if (!predicate())
			return;

		this.innerField.Write(writer, context);
	}

	public Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> predicate() ? this.innerField.ReadAsync(reader, context, cancellationToken) : Task.CompletedTask;

	public Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> predicate() ? this.innerField.WriteAsync(writer, context, cancellationToken: cancellationToken) : Task.CompletedTask;
}