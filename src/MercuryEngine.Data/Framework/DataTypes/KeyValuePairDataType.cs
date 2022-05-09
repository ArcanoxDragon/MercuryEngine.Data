namespace MercuryEngine.Data.Framework.DataTypes;

public class KeyValuePairDataType<TKey, TValue> : IBinaryDataType
where TKey : IBinaryDataType
where TValue : IBinaryDataType
{
	public KeyValuePairDataType(TKey initialKey, TValue initialValue)
	{
		Key = initialKey;
		Value = initialValue;
	}

	public TKey   Key   { get; }
	public TValue Value { get; }

	public uint Size => Key.Size + Value.Size;

	public void Read(BinaryReader reader)
	{
		Key.Read(reader);
		Value.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		Key.Write(writer);
		Value.Write(writer);
	}

	public void Deconstruct(out TKey key, out TValue value)
	{
		key = Key;
		value = Value;
	}
}