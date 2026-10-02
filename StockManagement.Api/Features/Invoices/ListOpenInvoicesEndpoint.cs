using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Invoices;


public sealed record ListOpenInvoicesRequest(int? CustomerId, string? Cursor, int? PageSize);


/// <remarks>Open, partly paid and overdue invoices; soonest due date first. Filter by <see cref="ListOpenInvoicesRequest.CustomerId"/> for one customer's open items.</remarks>
public class ListOpenInvoicesEndpoint(IPaymentService paymentService) : Endpoint<ListOpenInvoicesRequest, InvoiceListResponse>
{
	private readonly IPaymentService _paymentService = paymentService;


	public override void Configure()
	{
		this.Get("/invoices/open");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<InvoiceListResponse> ExecuteAsync(ListOpenInvoicesRequest request, CancellationToken cancellationToken)
	{
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _paymentService.GetOpenInvoicesAsync(request.CustomerId, request.Cursor, pageSize, cancellationToken);

		return new(result.Items.Select(invoice => InvoiceResponse.From(invoice, _paymentService)).ToList(), result.NextCursor);
	}
}
