using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Machine : StockItem
{
	public Machine() : base()
	{
	}

	public Machine(string name, string description = "", int amount = 1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None)
		: base(name: name, description:description, amount:amount, price:price, manufacturer:manufacturer)
	{
	}

	internal override void Register()
	{
		MainManager.Instance.MachineManager.Register(this);
	}

	internal override void Update(Action callback)
	{
		MainManager.Instance.MachineManager.Update(this, callback);
	}
}