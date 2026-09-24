using StockManagement.Import.Core;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class DuplicateFilterTests
{
	[TestMethod]
	public void Split_AllNew_ReturnsAllAsUnique()
	{
		// Arrange
		List<string> candidates = ["A1", "B2"];

		// Act
		var result = DuplicateFilter.Split(candidates, [], code => code);

		// Assert
		CollectionAssert.AreEqual(candidates, result.Unique.ToList());
		Assert.AreEqual(0, result.Duplicates.Count);
	}

	[TestMethod]
	public void Split_KeyAlreadyStored_MarksCandidateAsDuplicate()
	{
		// Arrange
		List<string> candidates = ["A1", "B2", "C3"];
		List<string> existing = ["B2"];

		// Act
		var result = DuplicateFilter.Split(candidates, existing, code => code);

		// Assert
		CollectionAssert.AreEqual(new[] { "A1", "C3" }, result.Unique.ToList());
		CollectionAssert.AreEqual(new[] { "B2" }, result.Duplicates.ToList());
	}

	[TestMethod]
	public void Split_KeyRepeatedInImport_KeepsFirstCopy()
	{
		// Arrange
		var first = new KeyValuePair<string, int>("A1", 1);
		var second = new KeyValuePair<string, int>("A1", 2);
		var other = new KeyValuePair<string, int>("B2", 3);

		// Act
		var result = DuplicateFilter.Split([first, other, second], [], pair => pair.Key);

		// Assert
		CollectionAssert.AreEqual(new[] { first, other }, result.Unique.ToList());
		CollectionAssert.AreEqual(new[] { second }, result.Duplicates.ToList());
	}

	[TestMethod]
	public void Split_KeyRepeatedAndAlreadyStored_MarksEveryCopyAsDuplicate()
	{
		// Arrange
		List<string> candidates = ["A1", "A1"];
		List<string> existing = ["A1"];

		// Act
		var result = DuplicateFilter.Split(candidates, existing, code => code);

		// Assert
		Assert.AreEqual(0, result.Unique.Count);
		Assert.AreEqual(2, result.Duplicates.Count);
	}

	[TestMethod]
	public void Split_CaseInsensitiveComparer_TreatsKeysAsEqual()
	{
		// Arrange
		List<string> candidates = ["a1", "B2"];
		List<string> existing = ["A1"];

		// Act
		var result = DuplicateFilter.Split(candidates, existing, code => code, StringComparer.OrdinalIgnoreCase);

		// Assert
		CollectionAssert.AreEqual(new[] { "B2" }, result.Unique.ToList());
	}

	[TestMethod]
	public void Split_DefaultComparer_IsCaseSensitive()
	{
		// Arrange
		List<string> candidates = ["a1"];
		List<string> existing = ["A1"];

		// Act
		var result = DuplicateFilter.Split(candidates, existing, code => code);

		// Assert
		Assert.AreEqual(1, result.Unique.Count);
	}

	[TestMethod]
	public void Split_NullCandidates_ReturnsEmptyLists()
	{
		// Act
		var result = DuplicateFilter.Split<string, string>(null, ["A1"], code => code);

		// Assert
		Assert.AreEqual(0, result.Unique.Count);
		Assert.AreEqual(0, result.Duplicates.Count);
	}
}
