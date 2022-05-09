using JetBrains.Annotations;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

[PublicAPI]
public class CBlackboard_CSection : DataStructure<CBlackboard_CSection>
{
	public string Name { get; set; } = string.Empty;

	public List<Property> Properties { get; } = new();

	protected override void Describe(DataStructureBuilder<CBlackboard_CSection> builder)
		=> builder.String(m => m.Name)
				  .CrcLiteral("CBlackboard::CSection")
				  .Int32(1)
				  .CrcLiteral("dctProps")
				  .Array(m => m.Properties);

	[PublicAPI]
	public sealed class Property : DataStructure<Property>
	{
		public string Key { get; set; } = string.Empty;

		public IBinaryDataType? Data => DynamicData.RawData;

		private DynamicDreadDataType DynamicData { get; set; } = new();

		protected override void Describe(DataStructureBuilder<Property> builder)
			=> builder.String(m => m.Key)
					  .DynamicTypedField(m => m.DynamicData);
	}
}