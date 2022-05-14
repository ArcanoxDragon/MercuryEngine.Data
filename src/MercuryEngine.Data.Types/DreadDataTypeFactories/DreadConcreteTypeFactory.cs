using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadConcreteTypeFactory : BaseDreadDataTypeFactory<DreadConcreteType, IBinaryDataType>
{
	public static DreadConcreteTypeFactory Instance { get; } = new();

	protected override IBinaryDataType CreateDataType(DreadConcreteType dreadType)
		=> dreadType.CreateDataType();
}