using System.Text;
using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Import;


public sealed record GetImportBatchReportRequest(string Id);


/// <remarks>CSV of the batch's non-ready rows (row, status, message, parsed fields), so a bad import can be fixed and re-uploaded without re-opening the web screen (#58).</remarks>
public class GetImportBatchReportEndpoint(IImportBatchService importBatchService) : Endpoint<GetImportBatchReportRequest, Results<FileContentHttpResult, NotFound>>
{
	private readonly IImportBatchService _importBatchService = importBatchService;


	public override void Configure()
	{
		this.Get("/import/batches/{Id}/report");
		this.Permissions(Permission.StockItemsRead, Permission.CustomersRead, Permission.SalesRead);
	}

	public override async Task<Results<FileContentHttpResult, NotFound>> ExecuteAsync(GetImportBatchReportRequest request, CancellationToken cancellationToken)
	{
		if (await _importBatchService.GetAsync(request.Id, cancellationToken) is not ImportBatch batch) return TypedResults.NotFound();

		var csv = Encoding.UTF8.GetBytes(BuildCsv(batch));
		var fileName = $"{Path.GetFileNameWithoutExtension(batch.FileName)}-report.csv";
		return TypedResults.File(csv, "text/csv", fileName);
	}

	private static string BuildCsv(ImportBatch batch)
	{
		var lines = new List<string> { "Row,Status,Message,Data" };
		lines.AddRange(batch.Rows
			.Where(row => row.Status != ImportRowStatus.Ready)
			.OrderBy(row => row.RowNumber)
			.Select(row => string.Join(",", CsvField(row.RowNumber.ToString()), CsvField(row.Status.ToString()), CsvField(row.ErrorMessage ?? ""), CsvField(RowData(row.PayloadJson)))));
		return string.Join("\r\n", lines) + "\r\n";
	}

	private static string RowData(string? payloadJson)
	{
		if (payloadJson is null) return "";

		var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson)!;
		return string.Join("; ", fields.Select(field => $"{field.Key}={field.Value}"));
	}

	private static string CsvField(string value)
	{
		return value.IndexOfAny([',', '"', '\r', '\n']) < 0 ? value : $"\"{value.Replace("\"", "\"\"")}\"";
	}
}
