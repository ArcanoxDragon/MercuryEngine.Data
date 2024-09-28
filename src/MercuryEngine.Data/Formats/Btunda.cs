using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Btunda : BaseStandardFormat<Btunda, base__tunable__CTunableManager>
{
	public override string DisplayName => "BTUNDA";

	[JsonIgnore] // The property on the structure itself should be serialized instead of this one
	public IDictionary<string, base__tunable__CTunable?>? CheckpointData
	{
		get
		{
			if (Root.Tunables is null)
				return null;

			return new DictionaryAdapter<TerminatedStringField, DreadPointer<base__tunable__CTunable>, string, base__tunable__CTunable?>(
				Root.Tunables,
				bk => bk.Value,
				bv => bv.Value,
				ak => new TerminatedStringField(ak),
				av => new DreadPointer<base__tunable__CTunable>(av)
			);
		}
	}
}