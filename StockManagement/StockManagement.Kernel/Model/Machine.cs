using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;

internal class Machine : StockItem
{
	public Machine() : base()
	{
	}

	public Machine (string name, string description = "", int price = 0, ManufacturerType manufacturerType = ManufacturerType.None)
		: base (name, description, price, manufacturerType)
	{

	}
}
