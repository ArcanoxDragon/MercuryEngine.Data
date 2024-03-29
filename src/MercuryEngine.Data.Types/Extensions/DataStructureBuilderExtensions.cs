﻿using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DataTypes;
using MercuryEngine.Data.Types.Utility;

namespace MercuryEngine.Data.Types.Extensions;

[PublicAPI]
public static class DataStructureBuilderExtensions
{
	public static DataStructureBuilder<T> CrcLiteral<T>(this DataStructureBuilder<T> builder, string literalText)
	where T : DataStructure<T>
		=> builder.AddVirtualField(new UInt64DataType { Value = literalText.GetCrc64() }, $"<CRC: \"{literalText}\">");

	public static DataStructureBuilder<T> CrcLiteral<T>(this DataStructureBuilder<T> builder, string literalText, string description)
	where T : DataStructure<T>
		=> builder.AddVirtualField(new UInt64DataType { Value = literalText.GetCrc64() }, description);

	public static DataStructureBuilder<TStructure> MsePropertyBag<TStructure>(
		this DataStructureBuilder<TStructure> builder,
		Action<PropertyBagFieldBuilder<TStructure>> configure)
	where TStructure : DataStructure<TStructure>
		=> builder.PropertyBag(CrcPropertyKeyGenerator.Instance, () => new StrIdDataType(), configure, StrIdDataType.EqualityComparer);
}