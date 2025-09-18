using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
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

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
		=> GetField(dataStructure).Read(reader, context);

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context)
		=> GetField(dataStructure).Write(writer, context);

	public Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
		=> GetField(dataStructure).ReadAsync(reader, context, cancellationToken);

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
		=> GetField(dataStructure).WriteAsync(writer, context, cancellationToken: cancellationToken);
}