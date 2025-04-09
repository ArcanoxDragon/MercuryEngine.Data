using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a property that holds a direct <see cref="IBinaryField"/> instance.
/// </summary>
public class DirectPropertyFieldHandler<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TOwner
>(PropertyInfo property) : IFieldHandler
{
	private readonly Func<TOwner, IBinaryField?> getter = ReflectionUtility.GetGetter<TOwner, IBinaryField?>(property);

	public uint GetSize(IDataStructure dataStructure)
		=> GetField(dataStructure).Size;

	public IBinaryField GetField(IDataStructure dataStructure)
		=> this.getter((TOwner) dataStructure) ?? throw new InvalidOperationException(
			$"Property \"{property.Name}\" was null while reading or writing a field " +
			$"on {dataStructure.GetType().FullName}. {nameof(IBinaryField)} properties must " +
			$"never have a null value.");

	public void Reset(IDataStructure dataStructure)
		=> ( GetField(dataStructure) as IResettableField )?.Reset();

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader)
		=> GetField(dataStructure).Read(reader);

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer)
		=> GetField(dataStructure).Write(writer);

	public Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, CancellationToken cancellationToken)
		=> GetField(dataStructure).ReadAsync(reader, cancellationToken);

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> GetField(dataStructure).WriteAsync(writer, cancellationToken);
}