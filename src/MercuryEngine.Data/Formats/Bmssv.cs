using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DataTypes;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Extensions;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmssv : BinaryFormat<Bmssv>
{
	public override string DisplayName => "BMSSV";

	/// <summary>
	/// A dictionary of all Blackboard sections present in the BMSSV file.
	/// </summary>
	public Dictionary<string, CBlackboard__CSection> Sections { get; private set; } = new();

	private int Unknown1 { get; set; }
	private int Unknown2 { get; set; }
	private int Unknown3 { get; set; }

	/// <summary>
	/// A proxy property over the publically exposed <see cref="Sections"/> property.
	/// When writing data, a copy of Sections is created and the data types are converted as appropriate.
	/// When reading data, the reverse is performed and stored in Sections.
	/// </summary>
	private Dictionary<TerminatedStringDataType, DynamicDreadDataType> RawSections
	{
		get => Sections.ToDictionary(pair => new TerminatedStringDataType(pair.Key), pair => new DynamicDreadDataType(pair.Value));
		set => Sections = value.ToDictionary(pair => pair.Key.Value, pair => (CBlackboard__CSection) pair.Value.Data!);
	}

	protected override void Describe(DataStructureBuilder<Bmssv> builder)
		// TODO: Support CBlackboard too
		=> builder.CrcLiteral("CGameBlackboard")
				  .Property(m => m.Unknown1)
				  .CrcLiteral("Root")
				  .Property(m => m.Unknown2)
				  .CrcLiteral("hashSections")
				  .Dictionary(m => m.RawSections)
				  .CrcLiteral("dctDeltaValues")
				  .Property(m => m.Unknown3);
}