using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Fields;

internal class TypedFieldWrapper(string typeName, IBinaryField wrappedField) : ITypedDreadField, IDataMapperAware
{
	public string TypeName => typeName;
	public uint   Size     => wrappedField.Size;

	DataMapper? IDataMapperAware.DataMapper
	{
		get => ( wrappedField as IDataMapperAware )?.DataMapper;
		set
		{
			if (wrappedField is IDataMapperAware dataMapperAware)
				dataMapperAware.DataMapper = value;
		}
	}

	public void Read(BinaryReader reader) => wrappedField.Read(reader);
	public void Write(BinaryWriter writer) => wrappedField.Write(writer);

	public Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> wrappedField.ReadAsync(reader, cancellationToken);

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> wrappedField.WriteAsync(writer, cancellationToken);
}