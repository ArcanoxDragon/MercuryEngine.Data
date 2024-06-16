using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// A field on a data structure that does not get exposed on the <typeparamref name="TStructure"/> type itself,
/// but that is included in the binary data when the <typeparamref name="TStructure"/> is written to a binary format.
/// </summary>
public class DataStructureVirtualField<TStructure, TData>(
	Func<TData> dataTypeFactory,
	TData initialValue,
	string? description = null
) : BaseDataStructureField<TStructure, TData>(dataTypeFactory)
where TStructure : IDataStructure
where TData : class, IBinaryDataType
{
	private const string DefaultDescription = "<virtual>";

	public DataStructureVirtualField(Func<TData> dataTypeFactory, string? description = null)
		: this(dataTypeFactory, dataTypeFactory(), description) { }

	public override string FriendlyDescription { get; } = description ?? DefaultDescription;

	private TData Data { get; set; } = initialValue;

	public override void ClearData(TStructure structure)
	{
		// No-op
	}

	public override bool HasData(TStructure structure) => true;

	protected override TData GetData(TStructure structure) => Data;
	protected override void PutData(TStructure structure, TData data) => Data = data;
}