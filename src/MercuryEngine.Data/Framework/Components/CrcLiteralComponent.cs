using MercuryEngine.Data.Extensions;

namespace MercuryEngine.Data.Framework.Components;

public class CrcLiteralComponent : LiteralComponent
{
	public CrcLiteralComponent(string literalText)
		: base(BitConverter.GetBytes(literalText.GetCrc64()))
	{
		OriginalText = literalText;
	}

	public string OriginalText { get; }
}