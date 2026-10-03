using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


/// <summary>
/// Single row holding the pre-assigned SIFEN contingency CDC range (#149)
/// </summary>
public interface IContingencyCdcRangeServiceProvider
{
	/// <summary>
	/// The configured range, or <see langword="null"/> if none was ever set
	/// </summary>
	public Task<ContingencyCdcRange?> GetAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Replaces the range wholesale (new DNIT-granted block); resets the reservation cursor to <paramref name="rangeStart"/> and turns contingency mode off
	/// </summary>
	public Task SetRangeAsync(long rangeStart, long rangeEnd, CancellationToken cancellationToken = default);

	/// <summary>
	/// Operator declares the start or end of a contingency event
	/// </summary>
	/// <exception cref="InvalidOperationException">No range configured yet</exception>
	public Task SetActiveAsync(bool active, CancellationToken cancellationToken = default);

	/// <summary>
	/// Atomically reserves and returns the next document number, or <see langword="null"/> when contingency mode
	/// is off, no range is configured, or the range is exhausted
	/// </summary>
	public Task<long?> TryReserveNextAsync(CancellationToken cancellationToken = default);
}
