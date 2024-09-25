using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Extensions;

[PublicAPI]
public static class DataStructureBuilderExtensions
{
	public static DataStructureBuilder<T> CrcConstant<T>(this DataStructureBuilder<T> builder, string literalText, bool assertValueOnRead = true)
	where T : DataStructure<T>
		=> builder.CrcConstant(literalText, $"<CRC: \"{literalText}\">", assertValueOnRead);

	public static DataStructureBuilder<T> CrcConstant<T>(this DataStructureBuilder<T> builder, string literalText, string description, bool assertValueOnRead = true)
	where T : DataStructure<T>
		=> builder.Constant(literalText.GetCrc64(), description, assertValueOnRead);
}