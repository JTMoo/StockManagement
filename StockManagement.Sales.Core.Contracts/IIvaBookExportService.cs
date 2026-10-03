namespace StockManagement.Sales.Core.Contracts;


public interface IIvaBookExportService
{
	/// <summary>
	/// Invoices and credit notes dated in [<paramref name="from"/>, <paramref name="to"/>], newest first, as IVA book rows
	/// </summary>
	public Task<IReadOnlyList<IvaBookRow>> GetRowsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
