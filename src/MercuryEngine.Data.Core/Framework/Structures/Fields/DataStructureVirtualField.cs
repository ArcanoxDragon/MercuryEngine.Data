using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// A field on a data structure that does not get exposed on the <typeparamref name="TStructure"/> type itself,
/// but that is included in the binary data when the <typeparamref name="TStructure"/> is written to a binary format.
/// </summary>
public class DataStructureVirtualField<TStructure, TField>(
	Func<TField> fieldFactory,
	TField initialValue,
	string? description = null
) : BaseDataStructureField<TStructure, TField>(fieldFactory)
where TStructure : IDataStructure
where TField : class, IBinaryField
{
	private const string DefaultDescription = "<virtual>";

	public DataStructureVirtualField(Func<TField> fieldFactory, string? description = null)
		: this(fieldFactory, fieldFactory(), description) { }

	public override string FriendlyDescription { get; } = description ?? DefaultDescription;

	private TField Data { get; set; } = initialValue;

	public override void ClearData(TStructure structure)
	{
		// No-op
	}

	public override bool HasData(TStructure structure) => true;

	protected override TField GetFieldForStorage(TStructure structure) => Data;
	protected override void LoadFieldFromStorage(TStructure structure, TField data) => Data = data;
}