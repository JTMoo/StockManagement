using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Payments;


public sealed record ListPaymentsRequest(int Number);


public sealed record InvoicePaymentsResponse(IReadOnlyList<PaymentResponse> Items, decimal AmountPaid, decimal AmountDue, InvoiceStatus Status);


public class ListPaymentsEndpoint(IInvoiceServiceProvider invoiceServiceProvider, IPaymentService paymentService) : Endpoint<ListPaymentsRequest, Results<Ok<InvoicePaymentsResponse>, NotFound>>
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;
	private readonly IPaymentService _paymentService = paymentService;


	public override void Configure()
	{
		this.Get("/invoices/{Number}/payments");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<Results<Ok<InvoicePaymentsResponse>, NotFound>> ExecuteAsync(ListPaymentsRequest request, CancellationToken cancellationToken)
	{
		if (await _invoiceServiceProvider.GetInvoiceAync(request.Number) is not Invoice invoice) return TypedResults.NotFound();

		var items = (invoice.Payments ?? []).OrderBy(payment => payment.Date).Select(PaymentResponse.From).ToList();
		return TypedResults.Ok(new InvoicePaymentsResponse(items, _paymentService.GetAmountPaid(invoice), _paymentService.GetAmountDue(invoice), _paymentService.GetStatus(invoice, DateTime.Now)));
	}
}
