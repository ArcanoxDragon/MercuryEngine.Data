using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

public class Toc : BinaryFormat<Toc>
{
	public Toc()
	{
		Files = new DictionaryAdapter<StrId, UInt32Field, StrId, uint>(
			InternalFiles,
			bk => bk,
			bv => bv.Value,
			ak => ak,
			ak => new UInt32Field(ak)
		);
	}

	public override string DisplayName => "TOC";

	public IDictionary<StrId, uint> Files { get; }

	#region Private Data

	private Dictionary<StrId, UInt32Field> InternalFiles { get; } = [];

	#endregion

	#region Public Methods

	/// <summary>
	/// Gets the size of the file with the file path indicated by <paramref name="file"/> (a hashed value).
	/// </summary>
	/// <exception cref="KeyNotFoundException">A file with the given path hash does not exist in the TOC.</exception>
	public uint GetFileSize(StrId file)
		=> Files[file];

	/// <summary>
	/// Gets the size of the file with the file path indicated by <paramref name="file"/> (a hashed value), if it exists.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if the given file exists, otherwise <see langword="false"/>.
	/// </returns>
	public bool TryGetFileSize(StrId file, out uint fileSize)
		=> Files.TryGetValue(file, out fileSize);

	/// <summary>
	/// Sets the size of the file with the file path indicated by <paramref name="file"/> to the provided
	/// <paramref name="fileSize"/>. The file will be added to the TOC if it does not already exist.
	/// </summary>
	public void PutFileSize(StrId file, uint fileSize)
	{
		if (InternalFiles.TryGetValue(file, out var fileSizeField))
		{
			// Optimized, don't need to allocate new UInt32Field 
			fileSizeField.Value = fileSize;
			return;
		}

		Files[file] = fileSize;
	}

	#endregion

	protected override void Describe(DataStructureBuilder<Toc> builder)
	{
		builder.Dictionary(m => m.InternalFiles);
	}
}