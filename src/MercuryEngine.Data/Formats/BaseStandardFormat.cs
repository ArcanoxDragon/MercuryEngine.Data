using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Extensions;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

public abstract class BaseStandardFormat<TSelf, TRoot> : BinaryFormat<TSelf>
where TSelf : BaseStandardFormat<TSelf, TRoot>, new()
where TRoot : class, IBinaryField, new()
{
	public virtual string?      TypeName => default;
	public virtual FileVersion? Version  => default;

	public TRoot Root { get; set; } = new();

	private StrId?       StoredTypeName { get; set; }
	private FileVersion? StoredVersion  { get; set; }

	protected override void BeforeWrite()
	{
		if (TypeName != null)
			StoredTypeName ??= TypeName.GetCrc64();

		StoredVersion ??= Version;
	}

	protected override void AfterRead()
	{
		if (TypeName != null && StoredTypeName != null && StoredTypeName.Value != TypeName.GetCrc64())
			throw new IOException($"Type name mismatch in {GetType().Name}: Expected \"{TypeName}\", but found \"{StoredTypeName}\"");
		if (Version != null && StoredVersion != Version)
			throw new IOException($"Version mismatch in {GetType().Name}: Expected \"{Version}\", but found \"{StoredVersion}\"");
	}

	protected override void Describe(DataStructureBuilder<TSelf> builder)
	{
		builder.NullableRawProperty(m => m.StoredTypeName);
		builder.NullableRawProperty(m => m.StoredVersion);
		builder.CrcConstant("Root");
		builder.RawProperty(m => m.Root);
	}
}