using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;

internal class SparePartManager
{
	private List<SparePart> spareParts = new List<SparePart>();


	internal void Init()
	{
		this.spareParts.Clear();
	}
}
