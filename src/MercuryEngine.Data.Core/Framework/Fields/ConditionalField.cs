using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class ConditionalField<TField>(Func<bool> predicate, TField innerField) : IBinaryField
where TField : IBinaryField
{
	private TField innerField = innerField;

	[JsonIgnore]
	public uint Size => predicate() ? this.innerField.Size : 0;

	public void Read(BinaryReader reader)
	{
		if (!predicate())
			return;

		this.innerField.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		if (!predicate())
			return;

		this.innerField.Write(writer);
	}

	public Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> predicate() ? this.innerField.ReadAsync(reader, cancellationToken) : Task.CompletedTask;

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> predicate() ? this.innerField.WriteAsync(writer, cancellationToken) : Task.CompletedTask;
}