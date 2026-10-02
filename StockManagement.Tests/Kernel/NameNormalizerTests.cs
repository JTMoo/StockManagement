using StockManagement.Kernel.Util;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class NameNormalizerTests
{
	[TestMethod]
	public void Normalize_DifferentCaseAndAccents_ProducesSameKey()
	{
		// Act
		var first = NameNormalizer.Normalize("José Pérez");
		var second = NameNormalizer.Normalize("JOSE PEREZ");

		// Assert
		Assert.AreEqual(first, second);
	}

	[TestMethod]
	public void Normalize_DifferentWordOrder_ProducesSameKey()
	{
		// Act
		var first = NameNormalizer.Normalize("Perez Jose");
		var second = NameNormalizer.Normalize("Jose Perez");

		// Assert
		Assert.AreEqual(first, second);
	}

	[TestMethod]
	public void Normalize_CompanySuffixVariants_ProducesSameKey()
	{
		// Act
		var first = NameNormalizer.Normalize("Acme S.A.");
		var second = NameNormalizer.Normalize("Acme SA");

		// Assert
		Assert.AreEqual(first, second);
	}

	[TestMethod]
	public void Normalize_DistinctNames_ProduceDifferentKeys()
	{
		// Act
		var first = NameNormalizer.Normalize("Ann Miller");
		var second = NameNormalizer.Normalize("Bo Nolan");

		// Assert
		Assert.AreNotEqual(first, second);
	}

	[TestMethod]
	public void Normalize_Blank_ReturnsEmpty()
	{
		// Act
		var result = NameNormalizer.Normalize("   ");

		// Assert
		Assert.AreEqual(string.Empty, result);
	}
}
