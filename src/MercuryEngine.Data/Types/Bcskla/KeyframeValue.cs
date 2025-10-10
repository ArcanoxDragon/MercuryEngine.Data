using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bcskla;

public struct KeyframeValue : IBinaryField
{
	public KeyframeValue() { }

	public KeyframeValue(float value, float rate = 0f)
	{
		Value = value;
		Rate = rate;
	}

	public float Value { get; set; }
	public float Rate  { get; set; }

	public uint GetSize(uint startPosition) => 2 * sizeof(float);

	public void Read(BinaryReader reader, ReadContext context)
	{
		Value = reader.ReadSingle();
		Rate = reader.ReadSingle();
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		writer.Write(Value);
		writer.Write(Rate);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		Value = await reader.ReadSingleAsync(cancellationToken).ConfigureAwait(false);
		Rate = await reader.ReadSingleAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(Value, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Rate, cancellationToken).ConfigureAwait(false);
	}

	public void Deconstruct(out float value, out float rate)
	{
		value = Value;
		rate = Rate;
	}
}