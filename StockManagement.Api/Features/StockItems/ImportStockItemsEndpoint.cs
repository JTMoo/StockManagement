using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using StockManagement.Import.Core.Contracts;

namespace StockManagement.Api.Features.StockItems;


public sealed class ImportStockItemsRequest
{
	public IFormFile File { get; set; } = default!;
}


public class ImportStockItemsValidator : Validator<ImportStockItemsRequest>
{
	public ImportStockItemsValidator()
	{
		this.RuleFor(request => request.File).NotNull().Must(file => file.Length > 0);
	}
}


public sealed record StockItemImportResponse(string SheetName, int Imported, int Duplicates, IReadOnlyList<StockItemImportRowError> Errors);


public sealed record InvalidExcelFileResponse(string Reason);


/// <remarks>One call: parses the first worksheet, matches columns to <see cref="StockManagement.Kernel.Model.StockItem"/> properties by name, splits duplicates and imports (docs/adr/0009-web-stock-item-excel-import.md).</remarks>
public class ImportStockItemsEndpoint(IExcelStockItemParser excelStockItemParser, IStockItemImportService stockItemImportService, ILogger<ImportStockItemsEndpoint> logger)
	: Endpoint<ImportStockItemsRequest, Results<Ok<StockItemImportResponse>, BadRequest<InvalidExcelFileResponse>>>
{
	private readonly IExcelStockItemParser _excelStockItemParser = excelStockItemParser;
	private readonly IStockItemImportService _stockItemImportService = stockItemImportService;
	private readonly ILogger<ImportStockItemsEndpoint> _logger = logger;


	public override void Configure()
	{
		this.Post("/stock-items/import");
		this.AllowFileUploads();
		this.AllowAnonymous();
	}

	public override async Task<Results<Ok<StockItemImportResponse>, BadRequest<InvalidExcelFileResponse>>> ExecuteAsync(ImportStockItemsRequest request, CancellationToken cancellationToken)
	{
		ExcelParseResult parsed;
		await using (var stream = request.File.OpenReadStream())
		{
			try
			{
				parsed = await _excelStockItemParser.ParseAsync(stream, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Could not parse uploaded stock item Excel file {FileName}", request.File.FileName);
				return TypedResults.BadRequest(new InvalidExcelFileResponse(request.File.FileName));
			}
		}

		var split = await _stockItemImportService.SplitDuplicatesAsync(parsed.Items, cancellationToken);
		await _stockItemImportService.ImportAsync(split.Unique, cancellationToken);

		return TypedResults.Ok(new StockItemImportResponse(parsed.SheetName, split.Unique.Count, split.Duplicates.Count, parsed.Errors));
	}
}
