namespace MercuryEngine.Data.Exceptions;

public class DataValidationException : Exception
{
	public DataValidationException(long offset, string? message) : this(offset, null, message) { }

	public DataValidationException(long offset, long? length, string? message) : base(message)
	{
		Offset = offset;
		Length = length;
	}

	public long  Offset { get; }
	public long? Length { get; }
}