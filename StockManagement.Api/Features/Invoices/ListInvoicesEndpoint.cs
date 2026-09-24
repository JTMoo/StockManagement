using FastEndpoints;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Invoices;


public sealed record ListInvoicesRequest(int? CustomerId, DateTime? From, DateTime? To, int? Page, int? PageSize);


/// <param name="TotalCount">Matching invoices across every page, not just <paramref name="Items"/></param>
public sealed record InvoiceListResponse(IReadOnlyList<InvoiceResponse> Items, int TotalCount);


public class ListInvoicesEndpoint(IInvoiceServiceProvider invoiceServiceProvider) : Endpoint<ListInvoicesRequest, InvoiceListResponse>
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;


	public override void Configure()
	{
		this.Get("/invoices");
	}

	public override async Task<InvoiceListResponse> ExecuteAsync(ListInvoicesRequest request, CancellationToken cancellationToken)
	{
		var page = Math.Max(request.Page ?? 1, 1);
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _invoiceServiceProvider.GetInvoicesAsync(request.CustomerId, request.From, request.To, page, pageSize);

		return new(result.Items.Select(InvoiceResponse.From).ToList(), result.TotalCount);
	}
}
