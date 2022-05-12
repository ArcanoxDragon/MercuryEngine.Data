using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Definitions.DreadTypes;

public abstract class BaseDreadType
{
	public string TypeName { get; set; } = string.Empty;

	public abstract DreadTypeKind Kind { get; }

	public abstract IBinaryDataType CreateDataType();
}