namespace MercuryEngine.Data.TegraTextureLib.ImageProcessing;

internal struct EncodingBlock
{
	public ulong Low;
	public ulong High;

	public void Encode(ulong value, ref int offset, int bits)
	{
		if (offset >= 64)
		{
			this.High |= value << ( offset - 64 );
		}
		else
		{
			this.Low |= value << offset;

			if (offset + bits > 64)
			{
				int remainder = 64 - offset;
				this.High |= value >> remainder;
			}
		}

		offset += bits;
	}

	public readonly ulong Decode(ref int offset, int bits)
	{
		ulong value;
		ulong mask = bits == 64 ? ulong.MaxValue : ( 1UL << bits ) - 1;

		if (offset >= 64)
		{
			value = ( this.High >> ( offset - 64 ) ) & mask;
		}
		else
		{
			value = this.Low >> offset;

			if (offset + bits > 64)
			{
				int remainder = 64 - offset;
				value |= this.High << remainder;
			}

			value &= mask;
		}

		offset += bits;

		return value;
	}
}