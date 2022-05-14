using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public interface IDreadDataTypeFactory
{
	IBinaryDataType CreateDataType(IDreadType dreadType);
}