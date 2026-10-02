using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Sifen;


public sealed record StuckTransmissionResponse(string Number, DateTime Date, string CustomerName, decimal Total, TransmissionStatus TransmissionStatus, string Cdc)
{
	public static StuckTransmissionResponse From(Invoice invoice)
	{
		var customerName = invoice.Customer is Customer customer ? string.Join(" ", customer.Name, customer.Lastname).Trim() : "";
		return new(invoice.Number, invoice.Date, customerName, invoice.Total, invoice.TransmissionStatus, invoice.Cdc);
	}
}


public sealed record StuckTransmissionListResponse(IReadOnlyList<StuckTransmissionResponse> Items);


/// <remarks>Rejected/stuck-Error invoices (ADR-0031) - required surface, not optional: legally unresolved until cleared.</remarks>
public class ListStuckTransmissionsEndpoint(IInvoiceServiceProvider invoiceServiceProvider) : EndpointWithoutRequest<StuckTransmissionListResponse>
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;


	public override void Configure()
	{
		this.Get("/sifen/stuck-transmissions");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<StuckTransmissionListResponse> ExecuteAsync(CancellationToken cancellationToken)
	{
		var invoices = await _invoiceServiceProvider.GetStuckTransmissionsAsync(cancellationToken);
		return new(invoices.Select(StuckTransmissionResponse.From).ToList());
	}
}
