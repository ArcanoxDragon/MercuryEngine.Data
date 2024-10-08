﻿using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing <see cref="IBinaryField"/>s that are members of a <see cref="IDataStructure"/>.
/// </summary>
[PublicAPI]
public interface IFieldHandler
{
	/// <summary>
	/// Gets the size of the data that this handler's field currently represents in the provided <paramref name="dataStructure"/>.
	/// </summary>
	uint GetSize(IDataStructure dataStructure);

	/// <summary>
	/// The <see cref="IBinaryField"/> instance that would be written by this handler for the provided <paramref name="dataStructure"/>,
	/// if applicable.
	/// </summary>
	IBinaryField? GetField(IDataStructure dataStructure);

	/// <summary>
	/// Resets the handler's field in the provided <paramref name="dataStructure"/> to its "default" state.
	/// </summary>
	void Reset(IDataStructure dataStructure);

	void HandleRead(IDataStructure dataStructure, BinaryReader reader);
	void HandleWrite(IDataStructure dataStructure, BinaryWriter writer);

	Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, CancellationToken cancellationToken);
	Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, CancellationToken cancellationToken);
}