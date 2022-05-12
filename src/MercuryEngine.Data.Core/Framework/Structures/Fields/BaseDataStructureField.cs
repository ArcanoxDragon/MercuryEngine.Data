using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

public abstract class BaseDataStructureField<TStructure, TData> : IDataStructureField<TStructure>
where TStructure : IDataStructure
where TData : IBinaryDataType
{
	private readonly Func<TData> dataTypeFactory;

	protected BaseDataStructureField(Func<TData> dataTypeFactory)
	{
		this.dataTypeFactory = dataTypeFactory;
	}

	public abstract string FriendlyDescription { get; }

	public virtual uint GetSize(TStructure structure) => GetData(structure).Size;

	public virtual void Read(TStructure structure, BinaryReader reader)
	{
		var data = CreateDataType();

		data.Read(reader);
		PutData(structure, data);
	}

	public virtual void Write(TStructure structure, BinaryWriter writer)
	{
		var data = GetData(structure);

		data.Write(writer);
	}

	protected abstract TData GetData(TStructure structure);
	protected abstract void PutData(TStructure structure, TData data);

	protected virtual TData CreateDataType() => this.dataTypeFactory();
}