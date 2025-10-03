using System.Runtime.CompilerServices;
using System.Text;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Pkg;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Formats;

public partial class Pkg
{
	/// <summary>
	/// Returns a sequence of all <see cref="PackageFile"/>s in the PKG file contained in the provided <paramref name="pkgStream"/>.
	/// The data of individual files will not be loaded, meaning the <see cref="PackageFile.Data"/> field will be empty for all
	/// files.
	/// </summary>
	public static IEnumerable<PackageFile> EnumeratePackageFiles(Stream pkgStream)
	{
		using BinaryReader reader = new(pkgStream, Encoding.UTF8, leaveOpen: true);
		var context = new ReadContext(new HeapManager());

		// Skip past HeaderSize and DataSectionSize
		reader.ReadUInt32();
		reader.ReadUInt32();

		// Read the number of file entries
		var fileCount = reader.ReadUInt32();

		// Read each file entry (only the header, not the data!)
		for (var i = 0; i < fileCount; i++)
		{
			var file = new PackageFile { ReadFileData = false };

			file.Read(reader, context);

			// Track our position in case someone seeks this stream during enumeration
			var currentPosition = pkgStream.Position;

			yield return file;

			// Restore position
			pkgStream.Position = currentPosition;
		}
	}

	/// <inheritdoc cref="EnumeratePackageFiles"/>
	public static async IAsyncEnumerable<PackageFile> EnumeratePackageFilesAsync(Stream pkgStream, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using AsyncBinaryReader reader = new(pkgStream);
		var context = new ReadContext(new HeapManager());

		// Skip past HeaderSize and DataSectionSize
		await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		// Read the number of file entries
		var fileCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		// Read each file entry (only the header, not the data!)
		for (var i = 0; i < fileCount; i++)
		{
			var file = new PackageFile { ReadFileData = false };

			await file.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);

			// Track our position in case someone seeks this stream during enumeration
			var currentPosition = pkgStream.Position;

			yield return file;

			// Restore position
			pkgStream.Position = currentPosition;
		}
	}

	/// <summary>
	/// Opens a <see cref="Stream"/> for the given <paramref name="file"/> from the PKG file contained in the provided <paramref name="pkgStream"/>.
	/// The file must have been retrieved from the same unmodified PKG file data as is provided via the stream.
	/// </summary>
	/// <remarks>
	/// If <paramref name="keepOpen"/> is <see langword="false"/> (the default), the provided <paramref name="pkgStream"/> will be disposed when
	/// the returned <see cref="Stream"/> is disposed.
	/// </remarks>
	public static Stream OpenPackageFile(Stream pkgStream, PackageFile file, bool keepOpen = false)
	{
		var fileLength = (int) ( file.DataField.EndAddress - file.DataField.StartAddress );
		var slicedStream = new SlicedStream(pkgStream, file.DataField.StartAddress, fileLength, keepOpen) {
			HideRealPosition = true,
		};

		// Move the underlying stream to the start of the SlicedStream, so that reads immediately start at the file's data block
		slicedStream.Position = 0;

		return slicedStream;
	}

	/// <summary>
	/// Reads the data for the given <paramref name="file"/> from the PKG file contained in the provided <paramref name="pkgStream"/>.
	/// The file must have been retrieved from the same unmodified PKG file data as is provided via the stream.
	/// </summary>
	public static byte[] ReadPackageFileData(Stream pkgStream, PackageFile file)
	{
		var fileLength = (int) ( file.DataField.EndAddress - file.DataField.StartAddress );

		pkgStream.Position = file.DataField.StartAddress;

		var data = new byte[fileLength];
		var bytesRead = pkgStream.Read(data);

		if (bytesRead < fileLength)
			throw new IOException($"Expected to read {fileLength} bytes for file \"{file.Name}\", but only read {bytesRead}");

		return data;
	}

	/// <summary>
	/// Reads the data for the given <paramref name="file"/> from the PKG file contained in the provided <paramref name="pkgStream"/>.
	/// The file must have been retrieved from the same unmodified PKG file data as is provided via the stream.
	/// </summary>
	public static async Task<byte[]> ReadPackageFileDataAsync(Stream pkgStream, PackageFile file, CancellationToken cancellationToken = default)
	{
		var fileLength = (int) ( file.DataField.EndAddress - file.DataField.StartAddress );

		pkgStream.Position = file.DataField.StartAddress;

		var data = new byte[fileLength];
		var bytesRead = await pkgStream.ReadAsync(data, cancellationToken).ConfigureAwait(false);

		if (bytesRead < fileLength)
			throw new IOException($"Expected to read {fileLength} bytes for file \"{file.Name}\", but only read {bytesRead}");

		return data;
	}
}