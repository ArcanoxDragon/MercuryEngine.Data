using System.Diagnostics;

namespace MercuryEngine.Data.TegraTextureLib.Tests.Utility;

public class DebuggingMemoryStream : MemoryStream
{
	private readonly HashSet<long> breakpointsCache = [];

	public long[] Breakpoints
	{
		get;
		set
		{
			field = value;
			this.breakpointsCache.Clear();
			this.breakpointsCache.UnionWith(value);
		}
	} = [];

	public override void WriteByte(byte value)
	{
		var thisPosition = Position;

		base.WriteByte(value);

		if (this.breakpointsCache.Contains(thisPosition))
			Debugger.Break();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		var thisPosition = Position;

		base.Write(buffer, offset, count);

		if (this.breakpointsCache.Contains(thisPosition))
			Debugger.Break();
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		var thisPosition = Position;

		base.Write(buffer);

		if (this.breakpointsCache.Contains(thisPosition))
			Debugger.Break();
	}
}