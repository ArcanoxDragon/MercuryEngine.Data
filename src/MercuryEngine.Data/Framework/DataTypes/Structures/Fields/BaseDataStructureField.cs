namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

public abstract class BaseDataStructureField<TStructure, TData> : IDataStructureField
where TStructure : DataStructure<TStructure>
where TData : IBinaryDataType
{
	protected BaseDataStructureField(TStructure structure)
	{
		Structure = structure;
	}

	public uint Size => Data.Size;

	public abstract string FriendlyDescription { get; }

	protected TStructure Structure { get; }

	protected abstract TData Data { get; }

	public virtual void Read(BinaryReader reader)
		=> Data.Read(reader);

	public virtual void Write(BinaryWriter writer)
		=> Data.Write(writer);
}