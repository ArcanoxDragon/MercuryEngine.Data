using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Definitions.Extensions;

namespace MercuryEngine.Data.Definitions.DataTypes;

[PublicAPI]
public class CBlackboard_CSection : DataStructure<CBlackboard_CSection>
{
	public string Name { get; set; } = string.Empty;

	public List<Property> Properties { get; } = new();

	protected override void Describe(DataStructureBuilder<CBlackboard_CSection> builder)
		=> builder.Property(m => m.Name)
				  .CrcLiteral("CBlackboard::CSection")
				  .Literal(1)
				  .CrcLiteral("dctProps")
				  .Array(m => m.Properties);

	[PublicAPI]
	public sealed class Property : DataStructure<Property>
	{
		public string Key { get; set; } = string.Empty;

		public IBinaryDataType? Data => DynamicData?.Data;

		private DynamicDreadValue? DynamicData { get; set; }

		protected override void Describe(DataStructureBuilder<Property> builder)
			=> builder.Property(m => m.Key)
					  .DynamicTypedField(m => m.DynamicData);
	}
}