using ClosedXML.Excel;
using Moq;
using StockManagement.Import.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class CustomerImportTargetHandlerTests
{
	private readonly Mock<ICustomerServiceProvider> _customers = new();
	private readonly CustomerImportTargetHandler _handler;


	public CustomerImportTargetHandlerTests()
	{
		_handler = new CustomerImportTargetHandler(_customers.Object);
	}


	[TestMethod]
	public void Target_IsCustomers()
	{
		Assert.AreEqual(ImportTarget.Customers, _handler.Target);
	}

	[TestMethod]
	public async Task ParseAsync_HeadersMatchPropertyNames_ReadsCustomersWithRowNumbers()
	{
		// Arrange
		using var workbook = CreateWorkbook(["Name", "Lastname"], ["Ann", "Miller"], ["Bo", "Nolan"]);

		// Act
		var (sheetName, candidates, errors) = await _handler.ParseAsync(ToStream(workbook), null);

		// Assert
		Assert.AreEqual("Customers", sheetName);
		Assert.AreEqual(0, errors.Count);
		CollectionAssert.AreEqual(new[] { 2, 3 }, candidates.Select(candidate => candidate.Row).ToList());
		Assert.AreEqual("Ann", ((Customer)candidates[0].Candidate).Name);
	}

	[TestMethod]
	public void GetFields_ReturnsCustomerPropertyAndDisplayNames()
	{
		// Act
		var fields = _handler.GetFields();

		// Assert
		Assert.IsTrue(fields.Any(field => field.Name == nameof(Customer.Name)));
	}

	[TestMethod]
	public async Task DetectColumnsAsync_KnownAndUnknownHeaders_MatchesTheKnownOneOnly()
	{
		// Arrange
		using var workbook = CreateWorkbook(["Name", "Something Else"], ["Ann", "?"]);

		// Act
		var (sheetName, columns) = await _handler.DetectColumnsAsync(ToStream(workbook));

		// Assert
		Assert.AreEqual("Customers", sheetName);
		Assert.AreEqual(nameof(Customer.Name), columns.Single(column => column.Column == 1).MatchedFieldName);
		Assert.IsNull(columns.Single(column => column.Column == 2).MatchedFieldName);
	}

	[TestMethod]
	public async Task ParseAsync_ExplicitColumnMapping_UsesItInsteadOfHeaderNames()
	{
		// Arrange
		using var workbook = CreateWorkbook(["Vorname", "Nachname"], ["Ann", "Miller"]);
		var mapping = new Dictionary<int, string> { [1] = nameof(Customer.Name), [2] = nameof(Customer.Lastname) };

		// Act
		var (_, candidates, _) = await _handler.ParseAsync(ToStream(workbook), mapping);

		// Assert
		var customer = (Customer)candidates.Single().Candidate;
		Assert.AreEqual("Ann", customer.Name);
		Assert.AreEqual("Miller", customer.Lastname);
	}

	[TestMethod]
	public async Task ParseAsync_ValidRucWithoutHyphen_NormalizesIdentificationNumber()
	{
		// Arrange
		using var workbook = CreateWorkbook(["Name", "IdentificationNumber"], ["Ann", "19465203"]);

		// Act
		var (_, candidates, errors) = await _handler.ParseAsync(ToStream(workbook), null);

		// Assert
		Assert.AreEqual(0, errors.Count);
		Assert.AreEqual("1946520-3", ((Customer)candidates[0].Candidate).IdentificationNumber);
	}

	[TestMethod]
	public async Task ParseAsync_NonRucIdentificationNumber_ImportsAsEnteredWithoutError()
	{
		// Arrange: e.g. a plain C.I. with no check digit
		using var workbook = CreateWorkbook(["Name", "IdentificationNumber"], ["Ann", "ID-1"]);

		// Act
		var (_, candidates, errors) = await _handler.ParseAsync(ToStream(workbook), null);

		// Assert
		Assert.AreEqual(0, errors.Count);
		Assert.AreEqual("ID-1", ((Customer)candidates[0].Candidate).IdentificationNumber);
	}

	[TestMethod]
	public async Task ParseAsync_BlankIdentificationNumber_ImportsWithoutError()
	{
		// Arrange
		using var workbook = CreateWorkbook(["Name"], ["Ann"]);

		// Act
		var (_, candidates, errors) = await _handler.ParseAsync(ToStream(workbook), null);

		// Assert
		Assert.AreEqual(0, errors.Count);
		Assert.AreEqual(1, candidates.Count);
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_SameIdentificationNumberAsStored_IsDuplicate()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync([new Customer { IdentificationNumber = "ID-1" }]);
		object clash = new Customer { Name = "Again", IdentificationNumber = "id-1" };
		object fresh = new Customer { Name = "New", IdentificationNumber = "ID-2" };

		// Act
		var result = await _handler.SplitDuplicatesAsync([clash, fresh]);

		// Assert
		CollectionAssert.AreEqual(new[] { fresh }, result.Unique.ToList());
		CollectionAssert.AreEqual(new[] { clash }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_BlankIdentificationNumbers_NeverMatchEachOther()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync([]);
		object first = new Customer { Name = "A" };
		object second = new Customer { Name = "B" };

		// Act
		var result = await _handler.SplitDuplicatesAsync([first, second]);

		// Assert
		Assert.AreEqual(2, result.Unique.Count);
		Assert.AreEqual(0, result.Duplicates.Count);
	}

	[TestMethod]
	public async Task CommitAsync_NewCustomers_AssignsConsecutiveIdsAfterHighestStored()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync([new Customer { CustomerId = 1005 }]);
		object first = new Customer { Name = "A" };
		object second = new Customer { Name = "B" };

		// Act
		await _handler.CommitAsync([first, second]);

		// Assert
		Assert.AreEqual(1006, ((Customer)first).CustomerId);
		Assert.AreEqual(1007, ((Customer)second).CustomerId);
		_customers.Verify(provider => provider.AddManyCustomersAsync(It.Is<IList<Customer>>(list => list.SequenceEqual(new[] { first, second }))), Times.Once);
	}

	[TestMethod]
	public async Task CommitAsync_NoCandidates_DoesNotCallDatabase()
	{
		// Act
		await _handler.CommitAsync([]);

		// Assert
		_customers.Verify(provider => provider.AddManyCustomersAsync(It.IsAny<IList<Customer>>()), Times.Never);
	}

	[TestMethod]
	public async Task UndoAsync_KnownIds_DeletesEachCustomer()
	{
		// Arrange
		var customer = new Customer();
		_customers.Setup(provider => provider.GetCustomerByIdAsync("id-1")).ReturnsAsync(customer);

		// Act
		await _handler.UndoAsync([("id-1", customer)]);

		// Assert
		_customers.Verify(provider => provider.DeleteCustomerAsync(customer), Times.Once);
	}

	[TestMethod]
	public void SerializeThenDeserializeCandidate_RoundTripsFields()
	{
		// Arrange
		var customer = new Customer { Name = "Ann", IdentificationNumber = "ID-1" };

		// Act
		var restored = (Customer)_handler.DeserializeCandidate(_handler.SerializeCandidate(customer));

		// Assert
		Assert.AreEqual("Ann", restored.Name);
		Assert.AreEqual("ID-1", restored.IdentificationNumber);
	}


	private static XLWorkbook CreateWorkbook(string[] headers, params string[][] rows)
	{
		var workbook = new XLWorkbook();
		var worksheet = workbook.AddWorksheet("Customers");

		for (var column = 0; column < headers.Length; column++)
		{
			worksheet.Cell(1, column + 1).Value = headers[column];
		}

		for (var row = 0; row < rows.Length; row++)
		{
			for (var column = 0; column < rows[row].Length; column++)
			{
				worksheet.Cell(row + 2, column + 1).Value = rows[row][column];
			}
		}

		return workbook;
	}

	private static MemoryStream ToStream(XLWorkbook workbook)
	{
		var stream = new MemoryStream();
		workbook.SaveAs(stream);
		stream.Position = 0;
		return stream;
	}
}
