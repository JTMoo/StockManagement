using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class SparePart(string name, string code = "", string description = "", int amount = 1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None) : StockItem(name:name, code:code, description:description, amount:amount, price:price, manufacturer:manufacturer)
{
	public SparePart(string name, string location, string code = "", string description = "", int amount = 1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None)
		: this(name: name, code:code, description: description, amount:amount, price:price, manufacturer:manufacturer)
	{
		this.Location = location;
	}

	internal override void Register()
	{
		MainManager.Instance.SparePartManager.Register(this);
	}

	internal override void Update(Action callback)
	{
		MainManager.Instance.SparePartManager.Update(this, callback);
	}
}