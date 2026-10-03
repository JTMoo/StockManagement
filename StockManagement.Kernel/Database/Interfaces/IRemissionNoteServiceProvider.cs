using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


/// <summary>
/// Storage for <see cref="RemissionNote"/> (#162) - mirrors <see cref="IInvoiceServiceProvider"/>'s shape, minus
/// the stock-decrement path: a remission note documents a movement, it never changes <see cref="StockItem.Amount"/>.
/// </summary>
public interface IRemissionNoteServiceProvider
{
	public Task<IReadOnlyList<RemissionNote>> GetRemissionNotesAsync(CancellationToken cancellationToken = default);

	public Task<RemissionNote?> GetRemissionNoteAsync(string number, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores the remission note and its outbox row (<see cref="PendingRemisionTransmission"/>) in one transaction.
	/// </summary>
	/// <exception cref="Exceptions.RemissionNoteNumberAlreadyExistsException">Number already exists; nothing written</exception>
	public Task AddAsync(RemissionNote remissionNote, CancellationToken cancellationToken = default);
}
