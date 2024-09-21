using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.Utility;

namespace MercuryEngine.Data.Types.Fields;

public class StrId(ulong value) : UInt64Field(value)
{
	public StrId()
		: this(default(ulong)) { }

	public StrId(string value)
		: this(value.GetCrc64())
	{
		KnownStrings.Record(value);
	}

	public string StringValue => KnownStrings.Get(Value);

	public override string ToString()
	{
		if (KnownStrings.TryGet(Value, out var str))
			return str;

		return $"<UNKNOWN StrId: {Value.ToHexString()}>";
	}

	public static implicit operator StrId(string value) => new(value);
	public static implicit operator StrId(ulong value) => new(value);
}