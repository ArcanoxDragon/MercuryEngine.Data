namespace MercuryEngine.Data.Core.Framework.DataTypes;

/// <summary>
/// Base class for data types that maintain a managed value of type <typeparamref name="T"/>.
/// </summary>
public abstract class BaseDataType<T> : IBinaryDataType<T>, IEquatable<BaseDataType<T>>
where T : notnull
{
	public static IEqualityComparer<IBinaryDataType<T>> EqualityComparer { get; } = new EqualityComparerImpl();

	private T value; // Must use a backing field to avoid virtual member calls in the constructor

	protected BaseDataType(T initialValue)
	{
		this.value = initialValue;
	}

	public virtual T Value
	{
		get => this.value;
		set => this.value = value;
	}

	public abstract uint Size { get; }

	public abstract void Read(BinaryReader reader);
	public abstract void Write(BinaryWriter writer);

	public override string? ToString() => Value.ToString();

	#region Equality

	public virtual bool Equals(BaseDataType<T>? other)
		=> other is not null && EqualityComparer.Equals(this, other);

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;

		return Equals((BaseDataType<T>) obj);
	}

	public override int GetHashCode()
		=> EqualityComparer.GetHashCode(this);

	public static bool operator ==(BaseDataType<T>? left, BaseDataType<T>? right)
		=> EqualityComparer.Equals(left, right);

	public static bool operator !=(BaseDataType<T>? left, BaseDataType<T>? right)
		=> !EqualityComparer.Equals(left, right);

	private sealed class EqualityComparerImpl : IEqualityComparer<IBinaryDataType<T>>
	{
		public bool Equals(IBinaryDataType<T>? x, IBinaryDataType<T>? y)
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

		public int GetHashCode(IBinaryDataType<T> obj)
			=> EqualityComparer<T>.Default.GetHashCode(obj.Value);
	}

	#endregion
}