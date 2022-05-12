using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fields;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Definitions.DataTypes.Fields;

public class DynamicDreadDataField<TStructure> : IDataStructureField<TStructure>
where TStructure : IDataStructure
{
	private readonly PropertyInfo propertyInfo;

	public DynamicDreadDataField(Expression<Func<TStructure, DynamicDreadValue?>> propertyExpression)
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);
	}

	public string FriendlyDescription => $"<dynamic {this.propertyInfo.Name}>";

	public uint GetSize(TStructure structure)
		=> GetCurrentValue(structure)?.Data.Size ?? 0;

	public void Read(TStructure structure, BinaryReader reader)
	{
		var typeId = reader.ReadUInt64();
		var type = DreadTypeRegistry.FindType(typeId);
		var value = new DynamicDreadValue(type);

		value.Data.Read(reader);
		SetCurrentValue(structure, value);
	}

	public void Write(TStructure structure, BinaryWriter writer)
	{
		if (GetCurrentValue(structure) is not { } value)
			throw new InvalidOperationException($"The value of property \"{this.propertyInfo.Name}\" on \"{typeof(TStructure).Name}\" was null");

		writer.Write(value.TypeId);
		value.Data.Write(writer);
	}

	private DynamicDreadValue? GetCurrentValue(TStructure structure)
		=> (DynamicDreadValue?) this.propertyInfo.GetValue(structure);

	private void SetCurrentValue(TStructure structure, DynamicDreadValue value)
		=> this.propertyInfo.SetValue(structure, value);
}