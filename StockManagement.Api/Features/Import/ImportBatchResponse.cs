using System.Text.Json;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Import;


public sealed record ImportBatchRowResponse(int Row, ImportRowStatus Status, string? Message, IReadOnlyDictionary<string, JsonElement>? Fields);


public sealed record ImportBatchResponse(string Id, ImportTarget Target, string FileName, string SheetName, ImportBatchStatus Status, int ReadyCount, int DuplicateCount, int ErrorCount, IReadOnlyList<ImportBatchRowResponse> Rows)
{
	public static ImportBatchResponse From(ImportBatch batch)
	{
		return new(
			batch.Id,
			batch.Target,
			batch.FileName,
			batch.SheetName,
			batch.Status,
			batch.Rows.Count(row => row.Status == ImportRowStatus.Ready),
			batch.Rows.Count(row => row.Status == ImportRowStatus.Duplicate),
			batch.Rows.Count(row => row.Status == ImportRowStatus.Error),
			[.. batch.Rows.OrderBy(row => row.RowNumber).Select(row => new ImportBatchRowResponse(row.RowNumber, row.Status, row.ErrorMessage, ParseFields(row.PayloadJson)))]);
	}

	private static IReadOnlyDictionary<string, JsonElement>? ParseFields(string? payloadJson)
	{
		return payloadJson is null ? null : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);
	}
}
