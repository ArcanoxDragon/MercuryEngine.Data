using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public interface IDreadFieldFactory
{
	IBinaryField CreateField(IDreadType dreadType);
}