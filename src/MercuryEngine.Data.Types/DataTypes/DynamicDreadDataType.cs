using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Types.DataTypes;

[PublicAPI]
public class DynamicDreadDataType : IBinaryDataType
{
	public IBinaryDataType? Data => CurrentValue?.Data;

	public uint Size => CurrentValue?.Data.Size ?? 0;

	private DynamicDreadValue? CurrentValue { get; set; }

	public void Read(BinaryReader reader)
	{
		var typeId = reader.ReadUInt64();
		var type = DreadTypeRegistry.FindType(typeId);

		CurrentValue = new DynamicDreadValue(type);
		CurrentValue.Data.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		if (CurrentValue is null)
			// Write nothing
			return;

		writer.Write(CurrentValue.TypeId);
		CurrentValue.Data.Write(writer);
	}
}