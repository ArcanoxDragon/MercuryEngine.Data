using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.DataTypes;

[PublicAPI]
public class EnumDataType<T> : BaseDataType<T>
where T : struct, Enum
{
	private readonly Func<BinaryReader, T>   readFunction;
	private readonly Action<BinaryWriter, T> writeFunction;

	public EnumDataType() : this(default) { }

	public EnumDataType(T initialValue) : base(initialValue)
	{
		this.readFunction = CreateReadFunction();
		this.writeFunction = CreateWriteFunction();
	}

	public override uint Size => (uint) Unsafe.SizeOf<T>();

	public override void Read(BinaryReader reader)
		=> Value = this.readFunction(reader);

	public override void Write(BinaryWriter writer)
		=> this.writeFunction(writer, Value);

	#region Function Generation

	private Func<BinaryReader, T> CreateReadFunction()
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

	private Action<BinaryWriter, T> CreateWriteFunction()
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