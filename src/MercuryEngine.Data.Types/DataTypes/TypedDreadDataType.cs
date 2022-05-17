using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DataTypes;

[PublicAPI]
public class TypedDreadDataType : IBinaryDataType
{
	private const ulong NullTypeId = 0UL;

	public TypedDreadDataType() { }

	public TypedDreadDataType(IDreadDataType initialValue)
		: this(initialValue.TypeName, initialValue) { }

	public TypedDreadDataType(string initialTypeName, IBinaryDataType initialValue)
		: this(new TypedDreadValue(initialTypeName, initialValue)) { }

	public TypedDreadDataType(TypedDreadValue initialValue)
	{
		InnerValue = initialValue;
	}

	public IBinaryDataType? InnerData => InnerValue?.Data;

	public uint Size => InnerValue?.Data.Size ?? 0;

	private TypedDreadValue? InnerValue { get; set; }

	public void Read(BinaryReader reader)
	{
		var typeId = reader.ReadUInt64();

		if (typeId is NullTypeId)
		{
			// Read NULL; clear out inner value and return
			InnerValue = null;
			return;
		}

		InnerValue = DreadTypeRegistry.CreateValueFor(typeId);
		InnerValue.Data.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		if (InnerValue is null)
		{
			// Write an all-zeroes type ID to indicate NULL
			writer.Write(NullTypeId);
			return;
		}

		writer.Write(InnerValue.TypeId);
		InnerValue.Data.Write(writer);
	}
}