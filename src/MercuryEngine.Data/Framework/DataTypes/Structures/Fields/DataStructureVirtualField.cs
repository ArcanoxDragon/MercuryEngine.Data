﻿namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

/// <summary>
/// A field on a data structure that does not get exposed on the <typeparamref name="TStructure"/> type itself,
/// but that is included in the binary data when the <typeparamref name="TStructure"/> is written to a binary format.
/// </summary>
public class DataStructureVirtualField<TStructure, TData> : BaseDataStructureField<TStructure, TData>
where TStructure : IDataStructure
where TData : class, IBinaryDataType
{
	private const string DefaultDescription = "<virtual>";

	public DataStructureVirtualField(Func<TData> dataTypeFactory, string? description = null)
		: this(dataTypeFactory, dataTypeFactory(), description) { }

	public DataStructureVirtualField(Func<TData> dataTypeFactory, TData initialValue, string? description = null) : base(dataTypeFactory)
	{
		Data = initialValue;
		FriendlyDescription = description ?? DefaultDescription;
	}

	public override string FriendlyDescription { get; }

	private TData Data { get; set; }

	protected override TData GetData(TStructure structure)
		=> Data;

	protected override void PutData(TStructure structure, TData data)
	{
		Data = data;
	}
}