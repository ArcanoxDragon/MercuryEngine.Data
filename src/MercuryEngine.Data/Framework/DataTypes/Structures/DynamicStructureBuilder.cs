using JetBrains.Annotations;
using MercuryEngine.Data.Framework.DataAdapters;

namespace MercuryEngine.Data.Framework.DataTypes.Structures;

/// <summary>
/// Provides a fluent interface for defining the format of a <see cref="DynamicStructure"/>.
/// </summary>
[PublicAPI]
public abstract class DynamicStructureBuilder
{
	private protected DynamicStructureBuilder() { }

	#region String Fields

	public abstract DynamicStructureBuilder String(string fieldName);

	#endregion

	#region Numeric Fields

	public abstract DynamicStructureBuilder Int16(string fieldName);
	public abstract DynamicStructureBuilder UInt16(string fieldName);
	public abstract DynamicStructureBuilder Int32(string fieldName);
	public abstract DynamicStructureBuilder UInt32(string fieldName);
	public abstract DynamicStructureBuilder Int64(string fieldName);
	public abstract DynamicStructureBuilder UInt64(string fieldName);
	public abstract DynamicStructureBuilder Float(string fieldName);
	public abstract DynamicStructureBuilder Double(string fieldName);
	public abstract DynamicStructureBuilder Decimal(string fieldName);

	#endregion

	#region Sub-Structures

	public abstract DynamicStructureBuilder Structure<TStructure>(string fieldName, TStructure initialValue)
	where TStructure : class, IDataStructure;

	public abstract DynamicStructureBuilder Structure<TStructure>(string fieldName)
	where TStructure : class, IDataStructure, new();

	public abstract DynamicStructureBuilder Array<TStructure>(string fieldName)
	where TStructure : class, IDataStructure, new();

	#endregion

	#region Raw Fields

	public abstract DynamicStructureBuilder AddField<TData>(string fieldName, TData initialValue)
	where TData : class, IBinaryDataType;

	public abstract DynamicStructureBuilder AddField<TValue, TData>(string fieldName)
	where TValue : notnull
	where TData : class, IBinaryDataType<TValue>, new();

	public abstract DynamicStructureBuilder AddField<TValue, TData>(string fieldName, IDataAdapter<TValue, TData> dataAdapter)
	where TValue : notnull
	where TData : IBinaryDataType, new();

	public abstract DynamicStructureBuilder AddField<TValue, TData>(string fieldName, TData initialValue, IDataAdapter<TValue, TData> dataAdapter)
	where TValue : notnull
	where TData : IBinaryDataType;

	#endregion
}