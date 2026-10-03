using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Database.Interfaces;


/// <summary>
/// Outbox queue for SIFEN transmission of <see cref="RemissionNote"/>s (#162) - same contract as
/// <see cref="IPendingTransmissionServiceProvider"/>, kept separate because it points at a different document entity.
/// </summary>
public interface IPendingRemisionTransmissionServiceProvider
{
	public Task<IReadOnlyList<PendingRemisionTransmission>> GetDueAsync(DateTime asOf, int maxCount, CancellationToken cancellationToken = default);

	public Task MarkTerminalAsync(PendingRemisionTransmission transmission, TransmissionStatus status, string cdc, CancellationToken cancellationToken = default);

	public Task MarkErrorAsync(PendingRemisionTransmission transmission, string error, DateTime? nextAttemptAt, CancellationToken cancellationToken = default);
}
