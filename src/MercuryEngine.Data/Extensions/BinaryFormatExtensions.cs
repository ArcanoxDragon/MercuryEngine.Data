using JetBrains.Annotations;
using MercuryEngine.Data.Framework;
using MercuryEngine.Data.Framework.Components;

namespace MercuryEngine.Data.Extensions;

[PublicAPI]
public static class BinaryFormatExtensions
{
	public static BinaryFormat<T>.Builder CrcLiteral<T>(this BinaryFormat<T>.Builder builder, string literalText)
	where T : BinaryFormat<T>
		=> builder.AddField(new CrcLiteralComponent(literalText));
}