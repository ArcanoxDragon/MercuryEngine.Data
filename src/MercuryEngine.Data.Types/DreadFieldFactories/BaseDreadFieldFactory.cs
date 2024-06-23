using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public abstract class BaseDreadFieldFactory<TDreadType, TField> : IDreadFieldFactory
where TDreadType : IDreadType
where TField : IBinaryField
{
	public IBinaryField CreateField(IDreadType dreadType)
	{
		ArgumentNullException.ThrowIfNull(dreadType);

		if (dreadType is not TDreadType derivedDreadType)
			throw new InvalidOperationException($"{GetType().Name} does not support the Dread type \"{dreadType.GetType().Name}\".");

		return CreateField(derivedDreadType);
	}

	protected abstract TField CreateField(TDreadType dreadType);
}