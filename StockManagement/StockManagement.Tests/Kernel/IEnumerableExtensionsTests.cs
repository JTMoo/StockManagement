using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Tests.Kernel;


[TestClass]
public class IEnumerableExtensionsTests
{
	[TestMethod]
	public void Where_EmptyFilter_ReturnsAllElements()
	{
		// Assign
		var filter = new List<Func<string, bool>>();
		List<string> items = ["12", "13", "14"];

		// Act
		var result = items.Where(filter);

		// Assert
		Assert.IsTrue(items.SequenceEqual(result));
	}
}
