using System.Text;
using System.Text.Json;
using MercuryEngine.Data.DataTypes;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Utility.DreadTypeHelpers;
using MercuryEngine.Data.Utility.Json;

namespace MercuryEngine.Data.Utility;

public static class DreadTypes
{
	private static readonly JsonSerializerOptions JsonOptions = new() {
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(),
		Converters = {
			new DreadTypeConverter(),
		},
	};

	private static readonly Dictionary<ulong, string>         TypeIdMap = new();
	private static readonly Dictionary<string, BaseDreadType> DreadTypeDefinitions;

	static DreadTypes()
	{
		DreadTypeDefinitions = ParseDreadTypes();

		RegisterConcreteType<TEnabledOccluderCollidersMap>();
		RegisterConcreteType<LiquidVolumesDictionary>("base::global::CRntSmallDictionary<base::global::CStrId, base::spatial::CAABox2D>");
		RegisterConcreteType<OccluderVignettesDictionary>("base::global::CRntSmallDictionary<base::global::CStrId, bool>");
		RegisterConcreteType<OccluderVignettesDictionary>("base::global::CRntSmallDictionary<base::global::CStrId, bool>");
		RegisterConcreteType<CBreakableTileGroupComponent_TActorTileStatesMap>();
		RegisterConcreteType<minimapGrid_TMinimapVisMap>();
		RegisterConcreteType<CMinimapManager_TCustomMarkerDataMap>();
	}

	public static void RegisterConcreteType<T>()
	where T : IBinaryDataType, new()
		=> RegisterConcreteType<T>(typeof(T).Name.Replace("_", "::"));

	public static void RegisterConcreteType<T>(string name)
	where T : IBinaryDataType, new()
	{
		DreadTypeDefinitions[name] = new DreadConcreteType<T>(name);
		TypeIdMap[name.GetCrc64()] = name;
	}

	public static BaseDreadType? FindType(string name)
		=> DreadTypeDefinitions.TryGetValue(name, out BaseDreadType? type) ? type : null;

	public static BaseDreadType? FindType(ulong typeId)
	{
		if (!TypeIdMap.TryGetValue(typeId, out var typeName))
		{
			var hexDisplay = BitConverter.GetBytes(typeId).ToHexString();

			throw new KeyNotFoundException($"The type ID \"{typeId}\" ({hexDisplay}) did not refer to a known type");
		}

		return FindType(typeName);
	}

	private static Dictionary<string, BaseDreadType> ParseDreadTypes()
	{
		using var fileStream = ResourceHelper.OpenResourceFile("DataDefinitions/dread_types.json");
		using var reader = new StreamReader(fileStream, Encoding.UTF8);
		var jsonText = reader.ReadToEnd();
		var typesDictionary = JsonSerializer.Deserialize<Dictionary<string, BaseDreadType>>(jsonText, JsonOptions)
							  ?? throw new InvalidOperationException("Unable to read the type definition database!");

		foreach (var (typeName, type) in typesDictionary)
		{
			var typeId = typeName.GetCrc64();

			type.TypeName = typeName;
			TypeIdMap[typeId] = typeName;
		}

		return typesDictionary;
	}
}