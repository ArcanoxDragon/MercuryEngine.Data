using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

/// <summary>
/// Base class for data types that maintain a managed value of type <typeparamref name="T"/>.
/// </summary>
public abstract class BaseBinaryField<T>(T initialValue) : IBinaryField<T>, IEquatable<BaseBinaryField<T>>, IDataMapperAware
where T : notnull
{
	public static IEqualityComparer<IBinaryField<T>> EqualityComparer { get; } = new EqualityComparerImpl();

	public virtual T Value { get; set; } = initialValue;

	public abstract uint Size { get; }

	protected DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public abstract void Read(BinaryReader reader);
	public abstract void Write(BinaryWriter writer);

	public abstract Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default);
	public abstract Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default);

	public override string? ToString() => Value.ToString();

	#region Equality

	public virtual bool Equals(BaseBinaryField<T>? other)
		=> other is not null && EqualityComparer.Equals(this, other);

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;

		return Equals((BaseBinaryField<T>) obj);
	}

	public override int GetHashCode()
		=> EqualityComparer.GetHashCode(this);

	public static bool operator ==(BaseBinaryField<T>? left, BaseBinaryField<T>? right)
		=> EqualityComparer.Equals(left, right);

	public static bool operator !=(BaseBinaryField<T>? left, BaseBinaryField<T>? right)
		=> !EqualityComparer.Equals(left, right);

	private sealed class EqualityComparerImpl : IEqualityComparer<IBinaryField<T>>
	{
		public bool Equals(IBinaryField<T>? x, IBinaryField<T>? y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if (ReferenceEquals(x, null))
				return false;
			if (ReferenceEquals(y, null))
				return false;
			if (x.GetType() != y.GetType())
				return false;

			return Equals(x.Value, y.Value);
		}

		public int GetHashCode(IBinaryField<T> obj)
			=> EqualityComparer<T>.Default.GetHashCode(obj.Value);
	}

	#endregion
}