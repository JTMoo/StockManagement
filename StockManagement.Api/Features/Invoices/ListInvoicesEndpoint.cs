using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Invoices;


public sealed record ListInvoicesRequest(int? CustomerId, DateTime? From, DateTime? To, string? Cursor, int? PageSize);


/// <param name="NextCursor">Opaque cursor for the next page; <see langword="null"/> on the last page</param>
public sealed record InvoiceListResponse(IReadOnlyList<InvoiceResponse> Items, string? NextCursor);


public class ListInvoicesEndpoint(IInvoiceServiceProvider invoiceServiceProvider, IPaymentService paymentService) : Endpoint<ListInvoicesRequest, InvoiceListResponse>
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;
	private readonly IPaymentService _paymentService = paymentService;


	public override void Configure()
	{
		this.Get("/invoices");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<InvoiceListResponse> ExecuteAsync(ListInvoicesRequest request, CancellationToken cancellationToken)
	{
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _invoiceServiceProvider.GetInvoicesAsync(request.CustomerId, request.From, request.To, request.Cursor, pageSize);

		return new(result.Items.Select(invoice => InvoiceResponse.From(invoice, _paymentService)).ToList(), result.NextCursor);
	}
}
