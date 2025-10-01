namespace MercuryEngine.Data.Converters.Bcmdl;

/// <summary>
/// The type of an actor in the game.
/// </summary>
public enum ActorType
{
	Character,
	Event,
	Effect,
	Item,
	Prop,
	SpawnPoint,
	Weapon,
}

public static class ActorTypeExtensions
{
	public static string GetRomFsFolder(this ActorType actorType)
		=> actorType switch {
			ActorType.Character  => "characters",
			ActorType.Event      => "events",
			ActorType.Effect     => "fx",
			ActorType.Item       => "items",
			ActorType.Prop       => "props",
			ActorType.SpawnPoint => "spawnpoints",
			ActorType.Weapon     => "weapons",

			_ => throw new ArgumentException($"Invalid actor type \"{actorType}\"", nameof(actorType)),
		};
}