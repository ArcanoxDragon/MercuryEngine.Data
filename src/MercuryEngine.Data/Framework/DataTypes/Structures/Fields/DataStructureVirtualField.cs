namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

public class DataStructureVirtualField<TStructure, TData> : BaseDataStructureField<TStructure, TData>
where TStructure : DataStructure<TStructure>
where TData : class, IBinaryDataType
{
	private const string DefaultDescription = "<virtual>";

	public DataStructureVirtualField(TStructure structure, TData data, string description = DefaultDescription) : base(structure)
	{
		Data = data;
		FriendlyDescription = description;
	}

	public override string FriendlyDescription { get; }

	protected override TData Data { get; }
}