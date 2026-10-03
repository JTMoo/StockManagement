using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// Creates SIFEN Nota de Remisión Electrónica documents (#162) - the goods-movement document DNIT requires
/// independent of whether the movement is also an invoiced sale. Does not touch <see cref="StockItem.Amount"/>:
/// a remission note documents a movement, a sale already decremented stock when it happened.
/// </summary>
public interface IRemissionNoteService
{
	/// <summary>
	/// DNIT composite number for the next new remission note, scoped to the configured establishment/point of sale -
	/// own sequence, independent of <see cref="ISaleService.GetNextInvoiceNumberAsync"/>.
	/// </summary>
	public Task<string> GetNextNumberAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Builds and stores a numbered remission note, queuing it for SIFEN transmission.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">An item amount is not greater than 0</exception>
	public Task<RemissionNote> CreateAsync(Customer customer, IReadOnlyList<(StockItem StockItem, int Amount)> items, RemissionReason reason, string destinationAddress, DateTime date, CancellationToken cancellationToken = default);
}
