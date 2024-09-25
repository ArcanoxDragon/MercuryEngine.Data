using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework;

/// <summary>
/// Defines the name of a property bag property that is associated with a C# class property.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public class PropertyNameAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}