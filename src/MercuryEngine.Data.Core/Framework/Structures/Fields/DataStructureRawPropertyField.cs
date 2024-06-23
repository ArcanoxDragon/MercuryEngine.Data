using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing a raw <see cref="IBinaryField"/> that
/// is a child of (e.g. property on) a <see cref="DataStructure{T}"/>.
/// </summary>
public class DataStructureRawPropertyField<TStructure, TField> : BaseDataStructureFieldWithProperty<TStructure, TField, TField?>
where TStructure : IDataStructure
where TField : class, IBinaryField
{
	public DataStructureRawPropertyField(
		Func<TField> fieldFactory,
		Expression<Func<TStructure, TField?>> propertyExpression
	) : base(fieldFactory, propertyExpression)
	{
		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureRawPropertyField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[{typeof(TField).Name}]";

	protected override TField GetFieldForStorage(TStructure structure)
	{
		var field = (TField?) PropertyInfo.GetValue(structure);

		if (field is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{PropertyInfo.Name}\" was null.");

		return field;
	}

	protected override void LoadFieldFromStorage(TStructure structure, TField data)
	{
		if (!PropertyInfo.CanWrite)
			return;

		PropertyInfo.SetValue(structure, data);
	}
}