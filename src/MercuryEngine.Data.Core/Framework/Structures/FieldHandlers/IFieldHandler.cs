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
	/// The size of the data that this handler's field currently represents.
	/// </summary>
	uint Size { get; }

	/// <summary>
	/// The <see cref="IBinaryField"/> instance that would be written by this handler, if applicable.
	/// </summary>
	IBinaryField? Field { get; }

	/// <summary>
	/// Resets the field to its "default" state.
	/// </summary>
	void Reset();

	void HandleRead(BinaryReader reader);
	void HandleWrite(BinaryWriter writer);

	Task HandleReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken);
	Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken);
}