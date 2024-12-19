namespace StockManagement.Kernel.Exceptions;


public class FailedExcelConversionException(ArgumentException argEx, int excelColumnNumber) : Exception(string.Concat(string.Format(Language.Resources.failedConversion, argEx.ParamName, excelColumnNumber), $" ({argEx.Message})"))
{
	public ArgumentException InnerArgumentException { get; private set; } = argEx;
}
