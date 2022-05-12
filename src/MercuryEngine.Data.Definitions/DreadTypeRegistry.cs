using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Definitions.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Json;
using MercuryEngine.Data.Resources.Utility;

namespace MercuryEngine.Data.Definitions;

public static class DreadTypeRegistry
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

	static DreadTypeRegistry()
	{
		DreadTypeDefinitions = ParseDreadTypes();

		RegisterConcreteType<TEnabledOccluderCollidersMap>();
		RegisterConcreteType<LiquidVolumesDictionary>("base::global::CRntSmallDictionary<base::global::CStrId, base::spatial::CAABox2D>");
		RegisterConcreteType<OccluderVignettesDictionary>("base::global::CRntSmallDictionary<base::global::CStrId, bool>");
		RegisterConcreteType<CBreakableTileGroupComponent_TActorTileStatesMap>();
		RegisterConcreteType<minimapGrid_TMinimapVisMap>();
		RegisterConcreteType<CMinimapManager_TCustomMarkerDataMap>();
		RegisterConcreteType<CMinimapManager_TGlobalMapIcons>();
		RegisterConcreteType<GUI_CMissionLog_TMissionLogEntries>();
		RegisterConcreteType("base::global::CRntVector<EMapTutoType>", ArrayDataType.Create<EnumDataType<EMapTutoType>>);
	}

	public static void RegisterConcreteType<T>()
	where T : IBinaryDataType, new()
		=> RegisterConcreteType<T>(typeof(T).Name.Replace("_", "::"));

	public static void RegisterConcreteType<T>(string name)
	where T : IBinaryDataType, new()
		=> RegisterConcreteType(name, () => new T());

	public static void RegisterConcreteType<T>(string name, Func<T> dataTypeFactory)
	where T : IBinaryDataType
	{
		DreadTypeDefinitions[name] = new DreadConcreteType<T>(dataTypeFactory, name);
		TypeIdMap[name.GetCrc64()] = name;
	}

	public static void RegisterDynamicType(string name, Action<DynamicStructureBuilder> buildStructure)
		=> RegisterConcreteType(name, () => DynamicStructure.Create(name, buildStructure));

	public static bool TryFindType(string name, [MaybeNullWhen(false)] out BaseDreadType type)
		=> DreadTypeDefinitions.TryGetValue(name, out type);

	public static BaseDreadType FindType(string name)
		=> TryFindType(name, out var type)
			   ? type
			   : throw new KeyNotFoundException($"The type \"{name}\" did not refer to a known type");

	public static bool TryFindType(ulong typeId, [MaybeNullWhen(false)] out BaseDreadType type)
	{
		if (!TypeIdMap.TryGetValue(typeId, out var typeName))
		{
			type = default;
			return false;
		}

		return TryFindType(typeName, out type);
	}

	public static BaseDreadType FindType(ulong typeId)
	{
		if (TryFindType(typeId, out var type))
			return type;

		var hexDisplay = BitConverter.GetBytes(typeId).ToHexString();

		throw new KeyNotFoundException($"The type ID \"{typeId}\" ({hexDisplay}) did not refer to a known type");
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