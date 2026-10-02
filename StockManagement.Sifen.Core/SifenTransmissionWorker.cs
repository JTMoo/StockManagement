using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Sifen.Core;


/// <summary>
/// Polls the <see cref="Kernel.Model.PendingTransmission"/> outbox and calls <see cref="ISifenGateway"/> for each
/// due row (ADR-0031). No message broker - single-process API, same reasoning as every other "no fire-and-forget" rule.
/// </summary>
public sealed class SifenTransmissionWorker(IServiceScopeFactory scopeFactory, ILogger<SifenTransmissionWorker> logger) : BackgroundService
{
	private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);
	private const int BatchSize = 20;

	private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
	private readonly ILogger<SifenTransmissionWorker> _logger = logger;


	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var timer = new PeriodicTimer(PollInterval);
		do
		{
			await this.ProcessDueAsync(stoppingToken);
		}
		while (await timer.WaitForNextTickAsync(stoppingToken));
	}

	private async Task ProcessDueAsync(CancellationToken cancellationToken)
	{
		await using var scope = _scopeFactory.CreateAsyncScope();
		var pendingTransmissions = scope.ServiceProvider.GetRequiredService<IPendingTransmissionServiceProvider>();
		var gateway = scope.ServiceProvider.GetRequiredService<ISifenGateway>();

		var due = await pendingTransmissions.GetDueAsync(DateTime.Now, BatchSize, cancellationToken);
		foreach (var transmission in due)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				var result = await gateway.SendAsync(transmission.Invoice, cancellationToken);
				await SifenTransmissionProcessor.ApplyAsync(pendingTransmissions, transmission, result, DateTime.Now, cancellationToken);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				_logger.LogError(ex, "SIFEN transmission failed for invoice {InvoiceNumber}", transmission.Invoice.Number);
			}
		}
	}
}
