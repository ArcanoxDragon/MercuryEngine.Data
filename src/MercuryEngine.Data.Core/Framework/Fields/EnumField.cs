﻿using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Overby.Extensions.AsyncBinaryReaderWriter;
using Task = System.Threading.Tasks.Task;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class EnumField<T>(T initialValue) : BaseBinaryField<T>(initialValue)
where T : struct, Enum
{
	// Dynamically-compiled read/write functions that will call the appropriate overload on BinaryReader/BinaryWriter
	// (these are necessary because different enums may have differently-sized backing data types)
	private static readonly Func<BinaryReader, T>   ReadFunction  = CreateReadFunction();
	private static readonly Action<BinaryWriter, T> WriteFunction = CreateWriteFunction();

	public EnumField() : this(default) { }

	[JsonIgnore]
	public override uint Size => (uint) Unsafe.SizeOf<T>();

	public override void Read(BinaryReader reader)
		=> Value = ReadFunction(reader);

	public override void Write(BinaryWriter writer)
		=> WriteFunction(writer, Value);

	public override Task ReadAsync(AsyncBinaryReader asyncReader, CancellationToken cancellationToken = default)
	{
		// We don't have the facilities by which to create an async lambda method for reading,
		// so we just have to read/write enums synchronously

		using var reader = new BinaryReader(asyncReader.BaseStream, Encoding.Default, true);

		Read(reader);

		return Task.CompletedTask;
	}

	public override async Task WriteAsync(AsyncBinaryWriter asyncWriter, CancellationToken cancellationToken = default)
	{
		// We don't have the facilities by which to create an async lambda method for reading,
		// so we just have to read/write enums synchronously

		var stream = await asyncWriter.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
		await using var writer = new BinaryWriter(stream, Encoding.Default, true);

		Write(writer);
	}

	#region Function Generation

	private static Func<BinaryReader, T> CreateReadFunction()
	{
		var underlyingType = Enum.GetUnderlyingType(typeof(T));

		if (underlyingType is null)
			throw new InvalidOperationException($"Unable to determine the underlying type of {typeof(T).Name}");

		var readMethods = typeof(BinaryReader).GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.Name.StartsWith("Read") && !m.Name.Contains("7BitEncoded") && m.Name != "Read")
			.ToList();
		var candidateReadMethod = readMethods.SingleOrDefault(m => m.ReturnType == underlyingType);

		if (candidateReadMethod is null)
			throw new InvalidOperationException($"Cannot find the appropriate Read method on {nameof(BinaryReader)} for the type \"{underlyingType.FullName}\"");

		// Build the function expression tree:
		//   (BinaryReader reader) => (T) reader.ReadXyz()
		var readerParameter = Expression.Parameter(typeof(BinaryReader), "reader");
		var readerReadCall = Expression.Call(readerParameter, candidateReadMethod);
		var cast = Expression.Convert(readerReadCall, typeof(T));
		var function = Expression.Lambda<Func<BinaryReader, T>>(cast, readerParameter);

		return function.Compile();
	}

	private static Action<BinaryWriter, T> CreateWriteFunction()
	{
		var underlyingType = Enum.GetUnderlyingType(typeof(T));

		if (underlyingType is null)
			throw new InvalidOperationException($"Unable to determine the underlying type of {typeof(T).Name}");

		var writeMethods = typeof(BinaryWriter).GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.Name == "Write")
			.ToList();
		var candidateWriteMethod = writeMethods.SingleOrDefault(m => m.GetParameters() is { Length: 1 } parameters && parameters[0].ParameterType == underlyingType);

		if (candidateWriteMethod is null)
			throw new InvalidOperationException($"Cannot find the appropriate Write method on {nameof(BinaryWriter)} for the type \"{underlyingType.FullName}\"");

		// Build the function expression tree:
		//   (BinaryWriter writer, T value) => writer.Write((TUnderlying) value)
		var writerParameter = Expression.Parameter(typeof(BinaryWriter), "writer");
		var valueParameter = Expression.Parameter(typeof(T), "value");
		var cast = Expression.Convert(valueParameter, underlyingType);
		var writerWriteCall = Expression.Call(writerParameter, candidateWriteMethod, cast);
		var function = Expression.Lambda<Action<BinaryWriter, T>>(writerWriteCall, writerParameter, valueParameter);

		return function.Compile();
	}

	#endregion
}