namespace MercuryEngine.Data.Framework.DataTypes;

/// <summary>
/// Base class for data types that maintain a managed value of type <typeparamref name="T"/>.
/// </summary>
public abstract class BaseDataType<T> : IBinaryDataType<T>
where T : notnull
{
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
}