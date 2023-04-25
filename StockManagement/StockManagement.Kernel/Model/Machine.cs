using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;

public class Machine : StockItem
{
	public Machine() : base()
	{
	}

	public Machine (string name, string description = "", int price = 0, ManufacturerType manufacturer = ManufacturerType.None)
		: base (name, description, price, manufacturer)
	{

	}

	internal override void Register()
	{
		MainManager.Instance.MachineManager.Register(this);
	}
}
