using System.Text.Json;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Kernel.Util;

namespace StockManagement.Import.Core;


/// <summary>
/// <see cref="IImportTargetHandler"/> for <see cref="Customer"/>: assigns the next free <see cref="Customer.CustomerId"/> per commit, same rule as <c>CustomerService.CreateCustomerAsync</c>
/// </summary>
/// <remarks>De-duplicates on <see cref="Customer.IdentificationNumber"/> only; a blank number never matches another, so most legacy rows import as new until a fuzzy check lands (#5).</remarks>
internal sealed class CustomerImportTargetHandler(ICustomerServiceProvider customerServiceProvider) : IImportTargetHandler
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public ImportTarget Target => ImportTarget.Customers;


	public async Task<(string SheetName, IReadOnlyList<(int Row, object Candidate)> Candidates, IReadOnlyList<ImportRowError> Errors)> ParseAsync(Stream excelFile, CancellationToken cancellationToken = default)
	{
		var (sheetName, items, errors) = await ExcelEntityParser<Customer>.ParseAsync(excelFile, cancellationToken);
		return (sheetName, [.. items.Select(item => (item.Row, (object)item.Item))], errors);
	}

	public async Task<DuplicateFilterResult<object>> SplitDuplicatesAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existing = await _customerServiceProvider.GetCustomersAsync() ?? [];
		var split = DuplicateFilter.Split(candidates.Cast<Customer>(), existing, DuplicateKey, StringComparer.OrdinalIgnoreCase);
		return new([.. split.Unique.Cast<object>()], [.. split.Duplicates.Cast<object>()]);
	}

	public async Task<IReadOnlyList<string>> CommitAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		var items = candidates.Cast<Customer>().ToList();
		if (items.Count == 0) return [];

		var existing = await _customerServiceProvider.GetCustomersAsync() ?? [];
		var nextId = SequenceNumber.Next(existing.Select(customer => customer.CustomerId), Customer.FirstCustomerId);
		foreach (var customer in items)
		{
			customer.CustomerId = nextId++;
		}

		await _customerServiceProvider.AddManyCustomersAsync(items);
		return [.. items.Select(item => item.Id)];
	}

	public async Task UndoAsync(IReadOnlyList<(string EntityId, object Candidate)> entities, CancellationToken cancellationToken = default)
	{
		foreach (var (id, _) in entities)
		{
			if (await _customerServiceProvider.GetCustomerByIdAsync(id) is Customer customer)
			{
				await _customerServiceProvider.DeleteCustomerAsync(customer);
			}
		}
	}

	public string SerializeCandidate(object candidate) => JsonSerializer.Serialize((Customer)candidate);

	public object DeserializeCandidate(string json) => JsonSerializer.Deserialize<Customer>(json)!;

	private static string DuplicateKey(Customer customer) =>
		string.IsNullOrWhiteSpace(customer.IdentificationNumber) ? Guid.NewGuid().ToString() : customer.IdentificationNumber;
}
