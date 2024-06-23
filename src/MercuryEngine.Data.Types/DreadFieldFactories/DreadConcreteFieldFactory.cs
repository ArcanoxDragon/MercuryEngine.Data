using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadConcreteFieldFactory : BaseDreadFieldFactory<DreadConcreteType, IBinaryField>
{
	public static DreadConcreteFieldFactory Instance { get; } = new();

	protected override IBinaryField CreateField(DreadConcreteType dreadType)
		=> dreadType.CreateDataType();
}