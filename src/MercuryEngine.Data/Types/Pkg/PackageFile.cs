using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Pkg;

public class PackageFile : DataStructure<PackageFile>
{
	public PackageFile()
	{
		DataField = new PackageFileData(this);
	}

	public StrId Name { get; set; } = new();

	[JsonIgnore]
	public byte[] Data
	{
		get => DataField.Data;
		set => DataField.Data = value;
	}

	[Obsolete($"This returns the size of the file's header entry, not the length of the file's data. Use {nameof(Length)} instead.")]
	public new uint GetSize(uint startPosition) => base.GetSize(startPosition);

	/// <summary>
	/// Gets the length of the file's data.
	/// </summary>
	public int Length => ReadFileData
		// Use the actual data length if data was read
		? DataField.Data.Length
		// Use the size of the data in the PKG, if it was read
		: (int) ( DataField.EndAddress - DataField.StartAddress );

	internal bool ReadFileData { get; set; } = true;

	#region Private Data

	internal PackageFileData DataField { get; }

	#endregion

	protected override void Describe(DataStructureBuilder<PackageFile> builder)
	{
		builder.RawProperty(m => m.Name);
		builder.RawProperty(m => m.DataField);
	}
}