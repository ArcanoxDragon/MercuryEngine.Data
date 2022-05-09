using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public abstract class BaseDreadType
{
	public string TypeName { get; set; } = string.Empty;

	public abstract DreadTypeKind Kind { get; }

	public abstract IBinaryDataType CreateDataType();
}