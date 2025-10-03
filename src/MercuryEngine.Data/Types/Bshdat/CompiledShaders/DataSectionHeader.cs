using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public interface IDataSectionHeader : IBinaryField;

public abstract class DataSectionHeader<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TSelf
> : DataStructure<TSelf>, IDataSectionHeader
where TSelf : DataSectionHeader<TSelf>
{
	private protected DataSectionHeader(DataSection parentSection)
	{
		ParentSection = parentSection;
	}

	internal DataSection ParentSection { get; }

	protected override void ReadCore(BinaryReader reader, ReadContext context)
	{
		base.ReadCore(reader, context);

		if (ParentSection is { DataSize: > 0, DataOffset: > 0 })
		{
			using (reader.BaseStream.TemporarySeek(ParentSection.DataOffset))
				ReadData(reader, context);
		}
	}

	protected override async Task ReadAsyncCore(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await base.ReadAsyncCore(reader, context, cancellationToken).ConfigureAwait(false);

		if (ParentSection is { DataSize: > 0, DataOffset: > 0 })
		{
			using (reader.BaseStream.TemporarySeek(ParentSection.DataOffset))
				await ReadDataAsync(reader, context, cancellationToken).ConfigureAwait(false);
		}
	}

	protected virtual void ReadData(BinaryReader reader, ReadContext context) { }
	protected virtual Task ReadDataAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken) => Task.CompletedTask;
}