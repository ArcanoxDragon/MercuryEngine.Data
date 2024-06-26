using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;
using MercuryEngine.Data.Types.Utility;

namespace MercuryEngine.Data.Types.Extensions;

[PublicAPI]
public static class DataStructureBuilderExtensions
{
	public static DataStructureBuilder<T> CrcConstant<T>(this DataStructureBuilder<T> builder, string literalText, bool assertValueOnRead = true)
	where T : DataStructure<T>, IDescribeDataStructure<T>
		=> builder.CrcConstant(literalText, $"<CRC: \"{literalText}\">", assertValueOnRead);

	public static DataStructureBuilder<T> CrcConstant<T>(this DataStructureBuilder<T> builder, string literalText, string description, bool assertValueOnRead = true)
	where T : DataStructure<T>, IDescribeDataStructure<T>
		=> builder.Constant(literalText.GetCrc64(), description, assertValueOnRead);

	public static DataStructureBuilder<T> MsePropertyBag<T>(this DataStructureBuilder<T> builder, Action<PropertyBagFieldBuilder<T>> configure)
	where T : DataStructure<T>, IDescribeDataStructure<T>
		=> builder.PropertyBag(CrcPropertyKeyGenerator.Instance, configure, StrId.EqualityComparer);
}