using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Database.Interfaces;


/// <summary>
/// Outbox queue for SIFEN transmission (ADR-0031). <see cref="Invoice.TransmissionStatus"/> is the source of
/// truth for a document's SIFEN state; a <see cref="PendingTransmission"/> row only exists while a retry is due.
/// </summary>
public interface IPendingTransmissionServiceProvider
{
	/// <summary>
	/// Rows due for an attempt (<see cref="PendingTransmission.NextAttemptAt"/> at or before <paramref name="asOf"/>), oldest first
	/// </summary>
	public Task<IReadOnlyList<PendingTransmission>> GetDueAsync(DateTime asOf, int maxCount, CancellationToken cancellationToken = default);

	/// <summary>
	/// SIFEN gave a final answer: sets the invoice's <see cref="TransmissionStatus"/> (and <see cref="Invoice.Cdc"/> once
	/// known) and removes the outbox row, in one transaction. Use for <see cref="TransmissionStatus.Accepted"/> and
	/// <see cref="TransmissionStatus.Rejected"/> - neither retries automatically.
	/// </summary>
	public Task MarkTerminalAsync(PendingTransmission transmission, TransmissionStatus status, string cdc, CancellationToken cancellationToken = default);

	/// <summary>
	/// A network/timeout error: sets the invoice to <see cref="TransmissionStatus.Error"/>, records <paramref name="error"/>
	/// and either reschedules the row to <paramref name="nextAttemptAt"/> or removes it when that is <see langword="null"/>
	/// (the 72h transmission deadline has passed - surfaces as a stuck <see cref="TransmissionStatus.Error"/> invoice).
	/// </summary>
	public Task MarkErrorAsync(PendingTransmission transmission, string error, DateTime? nextAttemptAt, CancellationToken cancellationToken = default);
}
