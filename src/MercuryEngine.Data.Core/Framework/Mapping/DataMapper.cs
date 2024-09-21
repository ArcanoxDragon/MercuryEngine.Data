using System.Text.Json;
using System.Text.Json.Serialization;

namespace MercuryEngine.Data.Core.Framework.Mapping;

[JsonConverter(typeof(JsonConverter))]
public class DataMapper
{
	private readonly DataRange        rootRange   = new("Root", 0);
	private readonly Stack<DataRange> rangeStack  = [];
	private readonly Stack<ulong>     offsetStack = [];

	private ulong currentOffset;

	public DataMapper()
	{
		this.rangeStack.Push(this.rootRange);
	}

	private DataRange CurrentRange => this.rangeStack.Peek();

	public void Reset()
	{
		while (this.rangeStack.Count > 1)
			this.rangeStack.Pop();

		this.rootRange.InnerRanges.Clear();
	}

	public void PushRange(string description, ulong start)
	{
		var newRange = new DataRange(description, AdjustOffset(start));

		CurrentRange.InnerRanges.Add(newRange);
		this.rangeStack.Push(newRange);
	}

	public void PopRange(ulong end)
	{
		if (this.rangeStack.Count == 1)
			throw new InvalidOperationException("Cannot pop the root range");

		var adjustedEnd = AdjustOffset(end);

		CurrentRange.End = adjustedEnd;
		this.rootRange.End = adjustedEnd;
		this.rangeStack.Pop();
	}

	public IDisposable PushOffset(ulong offset)
	{
		this.offsetStack.Push(offset);
		this.currentOffset += offset;
		return new OffsetToken(this);
	}

	public IEnumerable<DataRange> GetContainingRanges(ulong location)
		=> FindContainingRanges(this.rootRange, location);

	private ulong AdjustOffset(ulong offset)
		=> offset + this.currentOffset;

	private void PopOffset()
	{
		if (this.offsetStack.TryPop(out var topOffset))
			this.currentOffset -= topOffset;
	}

	private static IEnumerable<DataRange> FindContainingRanges(DataRange parentRange, ulong location)
	{
		if (location < parentRange.Start || location > parentRange.End)
			yield break;

		yield return parentRange;

		foreach (var innerRange in parentRange.InnerRanges)
		foreach (var containingRange in FindContainingRanges(innerRange, location))
			yield return containingRange;
	}

	private sealed class JsonConverter : JsonConverter<DataMapper>
	{
		public override DataMapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> throw new NotSupportedException("Reading is not supported");

		public override void Write(Utf8JsonWriter writer, DataMapper value, JsonSerializerOptions options)
			=> JsonSerializer.Serialize(writer, value.rootRange, options);
	}

	private sealed class OffsetToken(DataMapper owner) : IDisposable
	{
		private bool disposed;

		public void Dispose()
		{
			if (this.disposed)
				return;

			owner.PopOffset();
			this.disposed = true;
		}
	}
}