using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Import;


public sealed class PreviewImportRequest
{
	public ImportTarget Target { get; set; }

	public IFormFile File { get; set; } = default!;
}


public class PreviewImportValidator : Validator<PreviewImportRequest>
{
	public PreviewImportValidator()
	{
		this.RuleFor(request => request.File).Must(file => file is { Length: > 0 });
	}
}


public sealed record InvalidExcelFileResponse(string Reason);


/// <remarks>Parses the first worksheet and splits duplicates, then stores the result as a new batch; nothing is written to <see cref="ImportTarget"/> until it is committed (docs/adr/0015-generic-import-pipeline.md).</remarks>
public class PreviewImportEndpoint(IImportBatchService importBatchService, ILogger<PreviewImportEndpoint> logger)
	: Endpoint<PreviewImportRequest, Results<Ok<ImportBatchResponse>, BadRequest<InvalidExcelFileResponse>>>
{
	private readonly IImportBatchService _importBatchService = importBatchService;
	private readonly ILogger<PreviewImportEndpoint> _logger = logger;


	public override void Configure()
	{
		this.Post("/import/batches");
		this.AllowFileUploads();
		this.Permissions(Permission.StockItemsWrite, Permission.CustomersWrite);
	}

	public override async Task<Results<Ok<ImportBatchResponse>, BadRequest<InvalidExcelFileResponse>>> ExecuteAsync(PreviewImportRequest request, CancellationToken cancellationToken)
	{
		await using var stream = request.File.OpenReadStream();

		try
		{
			var batch = await _importBatchService.PreviewAsync(request.Target, request.File.FileName, stream, cancellationToken);
			return TypedResults.Ok(ImportBatchResponse.From(batch));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Could not parse uploaded {Target} import file {FileName}", request.Target, request.File.FileName);
			return TypedResults.BadRequest(new InvalidExcelFileResponse(request.File.FileName));
		}
	}
}
