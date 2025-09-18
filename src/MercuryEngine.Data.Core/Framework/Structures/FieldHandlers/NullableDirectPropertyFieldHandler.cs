using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a nullable property that can hold a direct <see cref="IBinaryField"/> instance.
/// </summary>
public class NullableDirectPropertyFieldHandler<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TOwner,
	TField
>(PropertyInfo property, Func<TField> fieldFactory) : IFieldHandler
where TField : IBinaryField
{
	private readonly Func<TOwner, TField?>   getter = ReflectionUtility.GetGetter<TOwner, TField?>(property);
	private readonly Action<TOwner, TField?> setter = ReflectionUtility.GetSetter<TOwner, TField?>(property);

	public uint GetSize(IDataStructure dataStructure)
		=> GetField(dataStructure)?.Size ?? 0u;

	public IBinaryField? GetField(IDataStructure dataStructure)
		=> this.getter((TOwner) dataStructure);

	public void SetField(IDataStructure dataStructure, TField? field)
		=> this.setter((TOwner) dataStructure, field);

	public void Reset(IDataStructure dataStructure)
		=> SetField(dataStructure, default);

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
	{
		if (GetField(dataStructure) is not TField field)
		{
			field = fieldFactory();
			SetField(dataStructure, field);
		}

		field.Read(reader, context);
	}

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context)
		=> GetField(dataStructure)?.Write(writer, context);

	public Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		if (GetField(dataStructure) is not TField field)
		{
			field = fieldFactory();
			SetField(dataStructure, field);
		}

		return field.ReadAsync(reader, context, cancellationToken);
	}

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
		=> GetField(dataStructure)?.WriteAsync(writer, context, cancellationToken: cancellationToken) ?? Task.CompletedTask;
}