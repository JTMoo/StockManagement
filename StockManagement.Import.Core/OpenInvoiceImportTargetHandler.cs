using System.Text.Json;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Import.Core;


/// <summary>
/// <see cref="IImportTargetHandler"/> for open invoices (ADR-0034): legacy receivables against an already-existing <see cref="Customer"/>, matched by <see cref="OpenInvoiceRow.CustomerIdentificationNumber"/>; stored with <see cref="IInvoiceServiceProvider.AddInvoiceAsync"/>, no stock movement
/// </summary>
/// <remarks>A repeated/already-stored <see cref="OpenInvoiceRow.Number"/>, or a <see cref="OpenInvoiceRow.CustomerIdentificationNumber"/> matching no existing customer, both land in the pipeline's one <see cref="DuplicateFilterResult{T}.Duplicates"/> bucket (ADR-0019 trade-off).</remarks>
internal sealed class OpenInvoiceImportTargetHandler(ICustomerServiceProvider customerServiceProvider, IInvoiceServiceProvider invoiceServiceProvider) : IImportTargetHandler
{
	private const PaymentMethod SeededPaymentMethod = PaymentMethod.Other;

	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;


	public ImportTarget Target => ImportTarget.OpenInvoices;


	public IReadOnlyList<ImportField> GetFields() => ExcelEntityParser<OpenInvoiceRow>.GetFields();

	public Task<(string SheetName, IReadOnlyList<DetectedColumn> Columns)> DetectColumnsAsync(Stream excelFile, CancellationToken cancellationToken = default) =>
		ExcelEntityParser<OpenInvoiceRow>.DetectColumnsAsync(excelFile, cancellationToken);

	public async Task<(string SheetName, IReadOnlyList<(int Row, object Candidate)> Candidates, IReadOnlyList<ImportRowError> Errors)> ParseAsync(Stream excelFile, IReadOnlyDictionary<int, string>? columnMapping, CancellationToken cancellationToken = default)
	{
		var (sheetName, items, errors) = await ExcelEntityParser<OpenInvoiceRow>.ParseAsync(excelFile, columnMapping, cancellationToken);
		return (sheetName, [.. items.Select(item => (item.Row, (object)item.Item))], errors);
	}

	public async Task<DuplicateFilterResult<object>> SplitDuplicatesAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existingNumbers = (await _invoiceServiceProvider.GetInvoicesAsync())
			.Select(invoice => invoice.Number)
			.ToHashSet();
		var knownIdentificationNumbers = (await _customerServiceProvider.GetCustomersAsync())
			.Select(customer => customer.IdentificationNumber)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		var seenNumbers = new HashSet<int>();
		List<object> unique = [];
		List<object> duplicates = [];
		foreach (var candidate in candidates)
		{
			var row = (OpenInvoiceRow)candidate;
			var isNewNumber = !existingNumbers.Contains(row.Number) && seenNumbers.Add(row.Number);
			var hasMatchingCustomer = knownIdentificationNumbers.Contains(row.CustomerIdentificationNumber);
			(isNewNumber && hasMatchingCustomer ? unique : duplicates).Add(candidate);
		}

		return new(unique, duplicates);
	}

	public async Task<IReadOnlyList<string>> CommitAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		var customersByIdentificationNumber = (await _customerServiceProvider.GetCustomersAsync())
			.ToDictionary(customer => customer.IdentificationNumber, StringComparer.OrdinalIgnoreCase);

		List<string> invoiceIds = [];
		foreach (var row in candidates.Cast<OpenInvoiceRow>())
		{
			var invoice = new Invoice
			{
				Number = row.Number,
				Customer = customersByIdentificationNumber[row.CustomerIdentificationNumber],
				Date = row.Date,
				ExpirationDate = row.ExpirationDate,
				Total = row.Total,
				Tax = row.Tax,
				SaleCondition = SaleCondition.Credit,
				Items = []
			};
			if (row.AmountPaid > 0) invoice.Payments.Add(new Payment { Date = row.Date, Amount = row.AmountPaid, Method = SeededPaymentMethod });

			await _invoiceServiceProvider.AddInvoiceAsync(invoice);
			invoiceIds.Add(invoice.Id);
		}

		return invoiceIds;
	}

	public async Task UndoAsync(IReadOnlyList<(string EntityId, object Candidate)> entities, CancellationToken cancellationToken = default)
	{
		foreach (var (_, candidate) in entities)
		{
			var row = (OpenInvoiceRow)candidate;
			if (await _invoiceServiceProvider.GetInvoiceAync(row.Number) is Invoice invoice)
			{
				await _invoiceServiceProvider.DeleteInvoiceAsync(invoice);
			}
		}
	}

	public string SerializeCandidate(object candidate) => JsonSerializer.Serialize((OpenInvoiceRow)candidate);

	public object DeserializeCandidate(string json) => JsonSerializer.Deserialize<OpenInvoiceRow>(json)!;
}
