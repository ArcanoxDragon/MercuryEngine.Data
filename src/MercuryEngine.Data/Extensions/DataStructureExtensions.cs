using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.DataTypes;
using MercuryEngine.Data.DataTypes.Fields;
using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.Extensions;

[PublicAPI]
public static class DataStructureExtensions
{
	public static DataStructureBuilder<T> CrcLiteral<T>(this DataStructureBuilder<T> builder, string literalText)
	where T : DataStructure<T>
		=> builder.AddVirtualField(new UInt64DataType { Value = literalText.GetCrc64() }, $"<CRC: \"{literalText}\">");

	public static DataStructureBuilder<T> CrcLiteral<T>(this DataStructureBuilder<T> builder, string literalText, string description)
	where T : DataStructure<T>
		=> builder.AddVirtualField(new UInt64DataType { Value = literalText.GetCrc64() }, description);

	public static DataStructureBuilder<T> DynamicTypedField<T>(this DataStructureBuilder<T> builder, Expression<Func<T, DynamicDreadValue?>> propertyExpression)
	where T : DataStructure<T>
		=> builder.AddField(new DynamicDreadDataField<T>(propertyExpression));
}