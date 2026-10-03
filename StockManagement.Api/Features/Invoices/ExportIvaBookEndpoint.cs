using System.Globalization;
using System.Text;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Invoices;


public sealed record ExportIvaBookRequest(DateTime From, DateTime To);


/// <remarks>CSV handoff for Marangatu's monthly IVA filing (#123); column layout is unverified against DNIT's spec - see <see cref="IvaBookRow"/>.</remarks>
public class ExportIvaBookEndpoint(IIvaBookExportService ivaBookExportService) : Endpoint<ExportIvaBookRequest, Results<FileContentHttpResult, BadRequest>>
{
	private readonly IIvaBookExportService _ivaBookExportService = ivaBookExportService;


	public override void Configure()
	{
		this.Get("/invoices/iva-book");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<Results<FileContentHttpResult, BadRequest>> ExecuteAsync(ExportIvaBookRequest request, CancellationToken cancellationToken)
	{
		if (request.From > request.To) return TypedResults.BadRequest();

		var rows = await _ivaBookExportService.GetRowsAsync(request.From, request.To, cancellationToken);
		var csv = Encoding.UTF8.GetBytes(BuildCsv(rows));
		var fileName = $"iva-book-{request.From:yyyyMMdd}-{request.To:yyyyMMdd}.csv";
		return TypedResults.File(csv, "text/csv", fileName);
	}

	private static string BuildCsv(IReadOnlyList<IvaBookRow> rows)
	{
		var lines = new List<string> { "TipoComprobante,Numero,Fecha,RucCliente,NombreCliente,Gravado10,Iva10,Gravado5,Iva5,Exento,Total" };
		lines.AddRange(rows.Select(row => string.Join(",",
			row.DocumentTypeCode, CsvField(row.Number), row.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), CsvField(row.CustomerRuc), CsvField(row.CustomerName),
			Amount(row.Taxed10), Amount(row.Vat10), Amount(row.Taxed5), Amount(row.Vat5), Amount(row.Exempt), Amount(row.Total))));
		return string.Join("\r\n", lines) + "\r\n";
	}

	private static string Amount(decimal value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	private static string CsvField(string value)
	{
		return value.IndexOfAny([',', '"', '\r', '\n']) < 0 ? value : $"\"{value.Replace("\"", "\"\"")}\"";
	}
}
