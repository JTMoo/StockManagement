using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;

internal class TireManager
{
	private List<Tire> tire = new List<Tire>();


	internal void Init()
	{
		this.tire.Clear();
	}
}
