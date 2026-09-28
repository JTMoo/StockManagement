using StockManagement.Kernel.Database;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Sales.Core;


internal class PaymentService(IInvoiceServiceProvider invoiceServiceProvider, ISettingsService settingsService) : IPaymentService
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;
	private readonly ISettingsService _settingsService = settingsService;


	public decimal GetAmountPaid(Invoice invoice)
	{
		return InvoiceStatusCalculator.AmountPaid(invoice);
	}

	public decimal GetAmountDue(Invoice invoice)
	{
		return InvoiceStatusCalculator.AmountDue(invoice);
	}

	public InvoiceStatus GetStatus(Invoice invoice, DateTime asOf)
	{
		return InvoiceStatusCalculator.GetStatus(invoice, asOf);
	}

	public async Task<RecordPaymentResult> RecordPaymentAsync(int invoiceNumber, decimal amount, PaymentMethod method, DateTime date, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (await _invoiceServiceProvider.GetInvoiceAync(invoiceNumber) is not Invoice invoice) return RecordPaymentResult.Failure(RecordPaymentError.InvoiceNotFound);

		var companySettings = await _settingsService.GetCompanySettingsAsync(cancellationToken);
		var roundedAmount = Math.Round(amount, companySettings.CurrencyDecimalDigits, MidpointRounding.AwayFromZero);
		if (roundedAmount <= 0) return RecordPaymentResult.Failure(RecordPaymentError.InvalidAmount);
		if (roundedAmount > InvoiceStatusCalculator.AmountDue(invoice)) return RecordPaymentResult.Failure(RecordPaymentError.ExceedsAmountDue);

		var payment = new Payment { Date = date, Amount = roundedAmount, Method = method };
		invoice.Payments.Add(payment);
		await _invoiceServiceProvider.UpdateInvoiceAsync(invoice);

		return RecordPaymentResult.Success(payment);
	}

	public async Task<PagedResult<Invoice>> GetOpenInvoicesAsync(int? customerId, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		var now = DateTime.Now;
		var invoices = await this.LoadInvoicesAsync(customerId, cancellationToken);
		var open = invoices.Where(invoice => InvoiceStatusCalculator.GetStatus(invoice, now) != InvoiceStatus.Paid);
		return Paginate(open, page, pageSize);
	}

	public async Task<PagedResult<Invoice>> GetOverdueInvoicesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
	{
		var now = DateTime.Now;
		var invoices = await this.LoadInvoicesAsync(customerId: null, cancellationToken);
		var overdue = invoices.Where(invoice => InvoiceStatusCalculator.GetStatus(invoice, now) == InvoiceStatus.Overdue);
		return Paginate(overdue, page, pageSize);
	}

	private async Task<IEnumerable<Invoice>> LoadInvoicesAsync(int? customerId, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var invoices = await _invoiceServiceProvider.GetInvoicesAsync() ?? [];
		return customerId is int id ? invoices.Where(invoice => invoice.Customer?.CustomerId == id) : invoices;
	}

	private static PagedResult<Invoice> Paginate(IEnumerable<Invoice> invoices, int page, int pageSize)
	{
		var ordered = invoices.OrderBy(invoice => invoice.ExpirationDate).ToList();
		var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
		return new(items, ordered.Count);
	}
}
