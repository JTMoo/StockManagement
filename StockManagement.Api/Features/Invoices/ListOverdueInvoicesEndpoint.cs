using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Invoices;


public sealed record ListOverdueInvoicesRequest(int? Page, int? PageSize);


/// <remarks>Invoices with an amount due whose due date has passed; soonest due date first.</remarks>
public class ListOverdueInvoicesEndpoint(IPaymentService paymentService) : Endpoint<ListOverdueInvoicesRequest, InvoiceListResponse>
{
	private readonly IPaymentService _paymentService = paymentService;


	public override void Configure()
	{
		this.Get("/invoices/overdue");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<InvoiceListResponse> ExecuteAsync(ListOverdueInvoicesRequest request, CancellationToken cancellationToken)
	{
		var page = Math.Max(request.Page ?? 1, 1);
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _paymentService.GetOverdueInvoicesAsync(page, pageSize, cancellationToken);

		return new(result.Items.Select(invoice => InvoiceResponse.From(invoice, _paymentService)).ToList(), result.TotalCount);
	}
}
