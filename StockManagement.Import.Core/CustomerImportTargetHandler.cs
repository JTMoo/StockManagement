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
/// <remarks>De-duplicates on <see cref="Customer.IdentificationNumber"/>; a blank number falls back to the normalized full name (<see cref="NameNormalizer"/>), so most legacy rows still get a duplicate check.</remarks>
internal sealed class CustomerImportTargetHandler(ICustomerServiceProvider customerServiceProvider) : IImportTargetHandler
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public ImportTarget Target => ImportTarget.Customers;


	public IReadOnlyList<ImportField> GetFields() => ExcelEntityParser<Customer>.GetFields();

	public Task<(string SheetName, IReadOnlyList<DetectedColumn> Columns)> DetectColumnsAsync(Stream excelFile, CancellationToken cancellationToken = default) =>
		ExcelEntityParser<Customer>.DetectColumnsAsync(excelFile, cancellationToken);

	/// <remarks><see cref="Customer.IdentificationNumber"/> also holds a plain C.I. (no check digit); only a value that verifies as a RUC is normalized, everything else is imported as entered.</remarks>
	public async Task<(string SheetName, IReadOnlyList<(int Row, object Candidate)> Candidates, IReadOnlyList<ImportRowError> Errors)> ParseAsync(Stream excelFile, IReadOnlyDictionary<int, string>? columnMapping, CancellationToken cancellationToken = default)
	{
		var (sheetName, items, errors) = await ExcelEntityParser<Customer>.ParseAsync(excelFile, columnMapping, cancellationToken);

		foreach (var (_, item) in items)
		{
			if (RucValidator.TryNormalize(item.IdentificationNumber, out var normalized)) item.IdentificationNumber = normalized;
		}

		return (sheetName, [.. items.Select(item => (item.Row, (object)item.Item))], errors);
	}

	/// <remarks>Two independent keys: non-blank <see cref="Customer.IdentificationNumber"/> (as before), and the normalized full name (<see cref="NameNormalizer"/>) for everything else - checked against every existing customer's name, with or without an identification number, since that's the case a blank-number row actually needs catching.</remarks>
	public async Task<DuplicateFilterResult<object>> SplitDuplicatesAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existing = await _customerServiceProvider.GetCustomersAsync(cancellationToken) ?? [];
		var takenIds = new HashSet<string>(
			existing.Select(customer => customer.IdentificationNumber).Where(id => !string.IsNullOrWhiteSpace(id)),
			StringComparer.OrdinalIgnoreCase);
		var takenNames = new HashSet<string>(
			existing.Select(FullNameKey).Where(name => name.Length > 0),
			StringComparer.Ordinal);

		List<Customer> unique = [];
		List<Customer> duplicates = [];
		foreach (var customer in candidates.Cast<Customer>())
		{
			if (!string.IsNullOrWhiteSpace(customer.IdentificationNumber))
			{
				(takenIds.Add(customer.IdentificationNumber) ? unique : duplicates).Add(customer);
				continue;
			}

			var nameKey = FullNameKey(customer);
			var isDuplicate = nameKey.Length > 0 && !takenNames.Add(nameKey);
			(isDuplicate ? duplicates : unique).Add(customer);
		}

		return new([.. unique.Cast<object>()], [.. duplicates.Cast<object>()]);
	}

	public async Task<IReadOnlyList<string>> CommitAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		var items = candidates.Cast<Customer>().ToList();
		if (items.Count == 0) return [];

		var existing = await _customerServiceProvider.GetCustomersAsync(cancellationToken) ?? [];
		var nextId = SequenceNumber.Next(existing.Select(customer => customer.CustomerId), Customer.FirstCustomerId);
		foreach (var customer in items)
		{
			customer.CustomerId = nextId++;
		}

		await _customerServiceProvider.AddManyCustomersAsync(items, cancellationToken);
		return [.. items.Select(item => item.Id)];
	}

	public async Task UndoAsync(IReadOnlyList<(string EntityId, object Candidate)> entities, CancellationToken cancellationToken = default)
	{
		foreach (var (id, _) in entities)
		{
			if (await _customerServiceProvider.GetCustomerByIdAsync(id, cancellationToken) is Customer customer)
			{
				await _customerServiceProvider.DeleteCustomerAsync(customer, cancellationToken);
			}
		}
	}

	public string SerializeCandidate(object candidate) => JsonSerializer.Serialize((Customer)candidate);

	public object DeserializeCandidate(string json) => JsonSerializer.Deserialize<Customer>(json)!;

	private static string FullNameKey(Customer customer) => NameNormalizer.Normalize($"{customer.Name} {customer.Lastname}");
}
