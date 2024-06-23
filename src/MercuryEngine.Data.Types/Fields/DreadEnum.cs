using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Types.Attributes;

namespace MercuryEngine.Data.Types.Fields;

public class DreadEnum<T> : EnumField<T>, ITypedDreadField
where T : struct, Enum
{
	private static readonly Lazy<string> TypeNameLazy = new(DreadEnum<T>.GetTypeName);

	private static string GetTypeName()
	{
		var enumAttribute = typeof(T).GetCustomAttribute<DreadEnumAttribute>();

		return enumAttribute?.TypeName ?? "UNKNOWN";
	}

	public string TypeName => TypeNameLazy.Value;
}