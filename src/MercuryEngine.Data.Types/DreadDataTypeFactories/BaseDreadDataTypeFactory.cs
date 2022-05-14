using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public abstract class BaseDreadDataTypeFactory<TDreadType, TDataType> : IDreadDataTypeFactory
where TDreadType : IDreadType
where TDataType : IBinaryDataType
{
	public IBinaryDataType CreateDataType(IDreadType dreadType)
	{
		if (dreadType is null)
			throw new ArgumentNullException(nameof(dreadType));
		if (dreadType is not TDreadType derivedDreadType)
			throw new InvalidOperationException($"{GetType().Name} does not support the Dread type \"{dreadType.GetType().Name}\".");

		return CreateDataType(derivedDreadType);
	}

	protected abstract TDataType CreateDataType(TDreadType dreadType);
}