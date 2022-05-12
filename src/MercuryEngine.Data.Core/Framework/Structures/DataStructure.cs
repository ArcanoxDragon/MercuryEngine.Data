using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Core.Framework.Structures;

[PublicAPI]
public abstract class DataStructure<T> : IDataStructure, IBinaryDataType<T>
where T : DataStructure<T>
{
	private readonly Lazy<List<IDataStructureField<T>>> fieldsLazy;

	protected DataStructure()
	{
		this.fieldsLazy = new Lazy<List<IDataStructureField<T>>>(BuildFields);
	}

	protected IEnumerable<IDataStructureField<T>> Fields => this.fieldsLazy.Value;

	protected abstract void Describe(DataStructureBuilder<T> builder);

	public uint Size => (uint) Fields.Sum(f => f.GetSize((T) this));

	public void Read(BinaryReader reader)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				field.Read((T) this, reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.FriendlyDescription}) of {GetType().Name} (position: {reader.BaseStream.Position})", ex);
			}
		}
	}

	public void Write(BinaryWriter writer)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				field.Write((T) this, writer);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while writing field {i} ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	private List<IDataStructureField<T>> BuildFields()
	{
		var builder = new Builder();

		Describe(builder);

		return builder.Build();
	}

	#region IBinaryDataType<T> Explicit Implementation

	T IBinaryDataType<T>.Value
	{
		get => (T) this;
		set => throw new InvalidOperationException($"DataStructure types consumed as {nameof(IBinaryDataType<T>)} cannot be assigned a value.");
	}

	#endregion

	#region Builder Implementation

	private sealed class Builder : DataStructureBuilder<T>
	{
		public List<IDataStructureField<T>> Build() => Fields;
	}

	#endregion
}