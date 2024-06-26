using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Definitions.DreadTypes;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Fields;

internal class TypedFieldWrapper(string typeName, IBinaryField wrappedField) : ITypedDreadField, IDataMapperAware
{
	public TypedFieldWrapper(IDreadType dreadType)
		: this(dreadType.TypeName, DreadTypeRegistry.CreateFieldForType(dreadType)) { }

	public string       TypeName     { get; } = typeName;
	public IBinaryField WrappedField { get; } = wrappedField;

	[JsonIgnore]
	public uint Size => WrappedField.Size;

	DataMapper? IDataMapperAware.DataMapper
	{
		get => ( WrappedField as IDataMapperAware )?.DataMapper;
		set
		{
			if (WrappedField is IDataMapperAware dataMapperAware)
				dataMapperAware.DataMapper = value;
		}
	}

	public void Read(BinaryReader reader) => WrappedField.Read(reader);
	public void Write(BinaryWriter writer) => WrappedField.Write(writer);

	public Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> WrappedField.ReadAsync(reader, cancellationToken);

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> WrappedField.WriteAsync(writer, cancellationToken);
}