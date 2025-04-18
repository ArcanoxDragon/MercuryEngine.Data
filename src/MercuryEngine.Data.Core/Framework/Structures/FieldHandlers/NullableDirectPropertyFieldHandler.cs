﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
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

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader)
	{
		if (GetField(dataStructure) is not TField field)
		{
			field = fieldFactory();
			SetField(dataStructure, field);
		}

		field.Read(reader);
	}

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer)
		=> GetField(dataStructure)?.Write(writer);

	public Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		if (GetField(dataStructure) is not TField field)
		{
			field = fieldFactory();
			SetField(dataStructure, field);
		}

		return field.ReadAsync(reader, cancellationToken);
	}

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> GetField(dataStructure)?.WriteAsync(writer, cancellationToken) ?? Task.CompletedTask;
}