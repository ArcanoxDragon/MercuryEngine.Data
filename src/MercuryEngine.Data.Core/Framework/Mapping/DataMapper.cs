namespace MercuryEngine.Data.Core.Framework.Mapping;

public class DataMapper
{
	private readonly Stack<DataRange> rangeStack  = [];
	private readonly Stack<ulong>     offsetStack = [];

	private ulong currentOffset;

	public DataMapper()
	{
		this.rangeStack.Push(RootRange);
	}

	internal DataRange RootRange { get; } = new("Root", 0);

	private DataRange CurrentRange => this.rangeStack.Peek();

	public void Reset()
	{
		while (this.rangeStack.Count > 1)
			this.rangeStack.Pop();

		RootRange.InnerRanges.Clear();
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
		RootRange.End = adjustedEnd;
		this.rangeStack.Pop();
	}

	public IDisposable PushOffset(ulong offset)
	{
		this.offsetStack.Push(offset);
		this.currentOffset += offset;
		return new OffsetToken(this);
	}

	public IEnumerable<DataRange> GetContainingRanges(ulong location)
		=> FindContainingRanges(RootRange, location);

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