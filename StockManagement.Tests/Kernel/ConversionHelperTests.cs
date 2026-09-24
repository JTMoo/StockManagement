using StockManagement.Kernel.Util;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class ConversionHelperTests
{
	[TestMethod]
	[DataRow(0L, "CERO")]
	[DataRow(1L, "UNO")]
	[DataRow(9L, "NUEVE")]
	[DataRow(19L, "DIECINUEVE")]
	[DataRow(21L, "VEINTIUNO")]
	[DataRow(29L, "VEINTINUEVE")]
	[DataRow(31L, "TREINTA Y UNO")]
	[DataRow(40L, "CUARENTA")]
	[DataRow(99L, "NOVENTA Y NUEVE")]
	[DataRow(100L, "CIEN")]
	[DataRow(101L, "CIENTO UNO")]
	[DataRow(999L, "NOVECIENTOS NOVENTA Y NUEVE")]
	public void ConvertToWords_BelowThousand_ReturnsWords(long number, string expected)
	{
		// Act
		var result = ConversionHelper.ConvertToWords(number);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	[DataRow(1_000L, "MIL")]
	[DataRow(1_001L, "MIL UNO")]
	[DataRow(21_000L, "VEINTIÚN MIL")]
	[DataRow(31_000L, "TREINTA Y UN MIL")]
	[DataRow(100_000L, "CIEN MIL")]
	[DataRow(101_000L, "CIENTO UN MIL")]
	[DataRow(1_000_000L, "UN MILLÓN")]
	[DataRow(2_000_000L, "DOS MILLONES")]
	[DataRow(21_000_000L, "VEINTIÚN MILLONES")]
	[DataRow(1_500_000L, "UN MILLÓN QUINIENTOS MIL")]
	[DataRow(1_000_000_000L, "MIL MILLONES")]
	[DataRow(1_000_000_000_000L, "UN BILLÓN")]
	[DataRow(123_456_789L, "CIENTO VEINTITRÉS MILLONES CUATROCIENTOS CINCUENTA Y SEIS MIL SETECIENTOS OCHENTA Y NUEVE")]
	public void ConvertToWords_ThousandsAndMore_ReturnsWords(long number, string expected)
	{
		// Act
		var result = ConversionHelper.ConvertToWords(number);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	[DataRow(-1L)]
	[DataRow(1_000_000_000_000_000_000L)]
	public void ConvertToWords_OutOfRange_Throws(long number)
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => ConversionHelper.ConvertToWords(number));
	}
}
