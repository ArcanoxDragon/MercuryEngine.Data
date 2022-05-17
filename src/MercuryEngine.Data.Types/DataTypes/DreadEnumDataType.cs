using System.Reflection;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DataTypes;

public class DreadEnumDataType<T> : EnumDataType<T>, IDreadDataType
where T : struct, Enum
{
	private static readonly Lazy<string> TypeNameLazy = new(GetTypeName);

	private static string GetTypeName()
	{
		var enumAttribute = typeof(T).GetCustomAttribute<DreadEnumAttribute>();

		return enumAttribute?.TypeName ?? "UNKNOWN";
	}

	public string TypeName => TypeNameLazy.Value;
}