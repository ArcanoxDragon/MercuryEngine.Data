using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Utility;
using MercuryEngine.Data.Utility.DreadTypeHelpers;

namespace MercuryEngine.Data.DataTypes;

public class DynamicDreadDataType : IBinaryDataType
{
	private BaseDreadType? dreadType;

	public BaseDreadType? DreadType
	{
		get => this.dreadType;
		set
		{
			this.dreadType = value;
			RawData = this.dreadType?.CreateDataType();
		}
	}

	public IBinaryDataType? RawData { get; set; }

	public ulong TypeId
	{
		get => DreadType?.TypeName.GetCrc64() ?? Crc64.Empty;
		set
		{
			if (DreadTypes.FindType(value) is not { } dreadType)
				throw new ArgumentOutOfRangeException(nameof(value), "Invalid type ID");

			DreadType = dreadType;
		}
	}

	public uint Size => RawData?.Size ?? 0;

	public void Read(BinaryReader reader)
	{
		TypeId = reader.ReadUInt64();

		ThrowIfUninitialized();

		RawData.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		ThrowIfUninitialized();

		writer.Write(TypeId);
		RawData.Write(writer);
	}

	[MemberNotNull(nameof(DreadType))]
	[MemberNotNull(nameof(RawData))]
	private void ThrowIfUninitialized()
	{
		if (DreadType is null)
			throw new InvalidOperationException($"This {nameof(DynamicDreadDataType)} is not initialized: {nameof(DreadType)} is null");
		if (RawData is null)
			throw new InvalidOperationException($"This {nameof(DynamicDreadDataType)} is not initialized: {nameof(RawData)} is null");
	}
}