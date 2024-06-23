using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

public class NullableFieldValueAdapter<TField, TValue> : IFieldAdapter<TField, TValue?>
where TField : IBinaryField<TValue>
where TValue : struct
{
	public static NullableFieldValueAdapter<TField, TValue> Instance { get; } = new();

	public TValue? Get(TField storage)
		=> storage.Value;

	public void Put(ref TField storage, TValue? value)
	{
		if (!value.HasValue)
			throw new ArgumentNullException(nameof(value));

		storage.Value = value.Value;
	}
}