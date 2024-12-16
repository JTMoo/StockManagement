namespace StockManagement.Kernel.Exceptions;


public class FailedConversionException(string? message) : Exception(message)
{
}
