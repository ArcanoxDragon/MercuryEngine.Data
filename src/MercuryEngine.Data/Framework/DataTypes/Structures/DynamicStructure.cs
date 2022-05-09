using System.Dynamic;
using JetBrains.Annotations;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataAdapters;
using MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

namespace MercuryEngine.Data.Framework.DataTypes.Structures;

[PublicAPI]
public sealed class DynamicStructure : DynamicObject, IDataStructure
{
	public static DynamicStructure Create(string typeName, Action<DynamicStructureBuilder> buildStructure)
	{
		var structure = new DynamicStructure(typeName);
		var builder = new Builder(structure);

		buildStructure(builder);

		return structure;
	}

	private readonly Dictionary<string, IDynamicStructureField> fields     = new();
	private readonly List<string>                               fieldOrder = new();

	private DynamicStructure(string typeName)
	{
		TypeName = typeName;
	}

	public IEnumerable<IDynamicStructureField> Fields => this.fieldOrder.Select(name => this.fields[name]);

	public string TypeName { get; }

	public uint Size => (uint) this.fields.Values.Sum(f => f.Size);

	public void Read(BinaryReader reader)
	{
		foreach (var (i, fieldName) in this.fieldOrder.Pairs())
		{
			var field = this.fields[fieldName];

			try
			{
				field.Read(reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field \"{i}\" ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	public void Write(BinaryWriter writer)
	{
		foreach (var (i, fieldName) in this.fieldOrder.Pairs())
		{
			var field = this.fields[fieldName];

			try
			{
				field.Write(writer);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field \"{i}\" ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	#region DynamicObject

	public override IEnumerable<string> GetDynamicMemberNames()
		=> this.fieldOrder;

	public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		result = default;

		if (!this.fields.TryGetValue(binder.Name, out var field))
			return false;

		result = field.Value;
		return true;
	}

	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		if (!this.fields.TryGetValue(binder.Name, out var field))
			return false;

		field.Value = value;
		return true;
	}

	#endregion

	#region Builder

	private sealed class Builder : DynamicStructureBuilder
	{
		private readonly DynamicStructure owner;

		internal Builder(DynamicStructure owner)
		{
			this.owner = owner;
		}

		#region String Fields

		public override DynamicStructureBuilder String(string fieldName)
			=> AddField<string, TerminatedStringDataType>(fieldName);

		#endregion

		#region Numeric Fields

		public override DynamicStructureBuilder Int16(string fieldName)
			=> AddField<short, Int16DataType>(fieldName);

		public override DynamicStructureBuilder UInt16(string fieldName)
			=> AddField<ushort, UInt16DataType>(fieldName);

		public override DynamicStructureBuilder Int32(string fieldName)
			=> AddField<int, Int32DataType>(fieldName);

		public override DynamicStructureBuilder UInt32(string fieldName)
			=> AddField<uint, UInt32DataType>(fieldName);

		public override DynamicStructureBuilder Int64(string fieldName)
			=> AddField<long, Int64DataType>(fieldName);

		public override DynamicStructureBuilder UInt64(string fieldName)
			=> AddField<ulong, UInt64DataType>(fieldName);

		public override DynamicStructureBuilder Float(string fieldName)
			=> AddField<float, FloatDataType>(fieldName);

		public override DynamicStructureBuilder Double(string fieldName)
			=> AddField<double, DoubleDataType>(fieldName);

		public override DynamicStructureBuilder Decimal(string fieldName)
			=> AddField<decimal, DecimalDataType>(fieldName);

		#endregion

		#region Sub-Structures

		public override DynamicStructureBuilder Structure<TStructure>(string fieldName, TStructure initialValue)
			=> AddField(new DynamicStructureRawField<TStructure>(this.owner, fieldName, initialValue));

		public override DynamicStructureBuilder Structure<TStructure>(string fieldName)
			=> AddField(new DynamicStructureRawField<TStructure>(this.owner, fieldName, new TStructure()));

		public override DynamicStructureBuilder Array<TStructure>(string fieldName)
			=> AddField(new DynamicStructureCollectionField<TStructure>(this.owner, fieldName));

		#endregion

		#region Raw Fields

		public DynamicStructureBuilder AddField(IDynamicStructureField field)
		{
			var fieldName = field.FieldName;

			this.owner.fields.Add(fieldName, field);
			this.owner.fieldOrder.Add(fieldName);

			return this;
		}

		public override DynamicStructureBuilder AddField<TData>(string fieldName, TData initialValue)
			=> AddField(new DynamicStructureRawField<TData>(this.owner, fieldName, initialValue));

		public override DynamicStructureBuilder AddField<TValue, TData>(string fieldName)
			=> AddField(fieldName, new BinaryDataTypeWithValueAdapter<TValue, TData>());

		public override DynamicStructureBuilder AddField<TValue, TData>(string fieldName, IDataAdapter<TValue, TData> dataAdapter)
			=> AddField(new DynamicStructureField<TValue, TData>(this.owner, fieldName, new TData(), dataAdapter));

		public override DynamicStructureBuilder AddField<TValue, TData>(string fieldName, TData initialValue, IDataAdapter<TValue, TData> dataAdapter)
			=> AddField(new DynamicStructureField<TValue, TData>(this.owner, fieldName, initialValue, dataAdapter));

		#endregion
	}

	#endregion
}