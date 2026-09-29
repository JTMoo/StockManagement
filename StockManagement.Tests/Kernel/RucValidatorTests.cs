using StockManagement.Kernel.Util;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class RucValidatorTests
{
	[TestMethod]
	public void TryNormalize_ValidWithHyphen_ReturnsTrueAndNormalizes()
	{
		// Act
		var result = RucValidator.TryNormalize("5530638-1", out var normalized);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual("5530638-1", normalized);
	}

	[TestMethod]
	public void TryNormalize_ValidWithoutHyphen_ReturnsTrueAndNormalizes()
	{
		// Act
		var result = RucValidator.TryNormalize("19465203", out var normalized);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual("1946520-3", normalized);
	}

	[TestMethod]
	public void TryNormalize_WrongCheckDigit_ReturnsFalse()
	{
		// Act
		var result = RucValidator.TryNormalize("5530638-2", out var normalized);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(string.Empty, normalized);
	}

	[TestMethod]
	public void TryNormalize_TooShort_ReturnsFalse()
	{
		// Act
		var result = RucValidator.TryNormalize("7", out _);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void TryNormalize_BlankInput_ReturnsFalse(string? ruc)
	{
		// Act
		var result = RucValidator.TryNormalize(ruc, out _);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryNormalize_NonNumericCharactersIgnored_StillNormalizes()
	{
		// Act
		var result = RucValidator.TryNormalize("5.530.638-1", out var normalized);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual("5530638-1", normalized);
	}
}
