namespace StockManagement.Kernel.Model.Types;


/// <summary>
/// Entity kind an <see cref="ImportBatch"/> reads into
/// </summary>
public enum ImportTarget
{
	StockItems = 0,

	Customers,

	OpeningStock
}
