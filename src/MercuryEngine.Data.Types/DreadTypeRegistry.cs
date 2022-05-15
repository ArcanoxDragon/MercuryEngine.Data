using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Utility;
using MercuryEngine.Data.Types.DataTypes;
using MercuryEngine.Data.Types.DreadDataTypeFactories;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types;

public static partial class DreadTypeRegistry
{
	private static readonly Dictionary<Type, IDreadDataTypeFactory> FactoryMap = new();
	private static readonly Dictionary<ulong, string>               TypeIdMap  = new();
	private static readonly Dictionary<string, BaseDreadType>       DreadTypeDefinitions;

	static DreadTypeRegistry()
	{
		DreadTypeDefinitions = DreadTypeParser.ParseDreadTypes();

		foreach (var typeName in DreadTypeDefinitions.Keys)
			TypeIdMap[typeName.GetCrc64()] = typeName;

		RegisterFactory<DreadConcreteType>(DreadConcreteTypeFactory.Instance);
		RegisterFactory<DreadDictionaryType>(DreadDictionaryTypeFactory.Instance);
		RegisterFactory<DreadEnumType>(DreadEnumTypeFactory.Instance);
		RegisterFactory<DreadFlagsetType>(DreadFlagsetTypeFactory.Instance);
		RegisterFactory<DreadPointerType>(DreadPointerTypeFactory.Instance);
		RegisterFactory<DreadPrimitiveType>(DreadPrimitiveTypeFactory.Instance);
		RegisterFactory<DreadStructType>(DreadStructTypeFactory.Instance);
		RegisterFactory<DreadTypedefType>(DreadTypedefTypeFactory.Instance);
		RegisterFactory<DreadVectorType>(DreadVectorTypeFactory.Instance);

		RegisterGeneratedTypes();

		// Manual concrete types (TODO: Just put these as definitions in dread_types.json?)
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

	static partial void RegisterGeneratedTypes();

	#region Factory Registration

	public static void RegisterFactory<T>(IDreadDataTypeFactory factory)
	where T : IDreadType
		=> RegisterFactory(typeof(T), factory);

	public static void RegisterFactory(Type dreadType, IDreadDataTypeFactory factory)
		=> FactoryMap.Add(dreadType, factory);

	#endregion

	#region Factory Access

	public static IBinaryDataType CreateDataTypeFor(IDreadType dreadType)
	{
		var factory = GetFactoryFor(dreadType);

		if (factory is null)
			throw new InvalidOperationException($"No factory implementation registered for type \"{dreadType.GetType().FullName}\"");

		return factory.CreateDataType(dreadType);
	}

	private static IDreadDataTypeFactory? GetFactoryFor(IDreadType dreadType)
	{
		if (FactoryMap.TryGetValue(dreadType.GetType(), out var factory))
			return factory;

		// Iterate through factory and check for compatibility by inheritance
		foreach (var (type, factoryCandidate) in FactoryMap)
		{
			if (type.IsInstanceOfType(dreadType))
			{
				// Cache this exact relationship for faster lookup later
				FactoryMap.Add(dreadType.GetType(), factoryCandidate);
				return factoryCandidate;
			}
		}

		return null;
	}

	#endregion

	#region Concrete Type Registration

	public static void RegisterConcreteType<T>()
	where T : IBinaryDataType, new()
		=> RegisterConcreteType(() => new T());

	public static void RegisterConcreteType<T>(string typeName)
	where T : IBinaryDataType, new()
		=> RegisterConcreteType(typeName, () => new T());

	public static void RegisterConcreteType<T>(Func<T> concreteTypeFactory)
	where T : IBinaryDataType
		=> RegisterConcreteType(typeof(T).Name.Replace("_", "::"), concreteTypeFactory);

	public static void RegisterConcreteType<T>(string typeName, Func<T> concreteTypeFactory)
	where T : IBinaryDataType
	{
		TypeIdMap.TryAdd(typeName.GetCrc64(), typeName);
		DreadTypeDefinitions[typeName] = new DreadConcreteType(typeName, () => concreteTypeFactory());
	}

	#endregion

	#region Type Lookup

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

	#endregion
}