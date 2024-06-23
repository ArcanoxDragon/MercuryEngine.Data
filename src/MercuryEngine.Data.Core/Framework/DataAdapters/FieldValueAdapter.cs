using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

public class FieldValueAdapter<TBinaryValue, TValue> : IFieldAdapter<TBinaryValue, TValue>
where TBinaryValue : IBinaryField<TValue>
where TValue : notnull
{
	public static FieldValueAdapter<TBinaryValue, TValue> Instance { get; } = new();

	public TValue Get(TBinaryValue storage)
		=> storage.Value;

	public void Put(ref TBinaryValue storage, TValue value)
		=> storage.Value = value;
}