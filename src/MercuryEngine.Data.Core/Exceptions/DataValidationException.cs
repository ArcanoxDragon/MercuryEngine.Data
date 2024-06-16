namespace MercuryEngine.Data.Core.Exceptions;

public class DataValidationException(long offset, long? length, string? message) : Exception(message)
{
	public DataValidationException(long offset, string? message)
		: this(offset, null, message) { }

	public long  Offset { get; } = offset;
	public long? Length { get; } = length;
}