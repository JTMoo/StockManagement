using System.Text.RegularExpressions;

namespace StockManagement.Kernel.Exceptions;


public class FailedExcelConversionException : Exception
{
	public FailedExcelConversionException(ArgumentException argEx, int excelColumnNumber)
		: base(string.Concat(string.Format(Language.Resources.failedConversion, argEx.ParamName, excelColumnNumber), $" ({argEx.Message})"))
	{
		this.InnerArgumentException = argEx;
	}


	public ArgumentException InnerArgumentException { get; private set; }
}
