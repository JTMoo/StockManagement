using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;

public class SparePart : StockItem
{
	public SparePart(string name, Location location, string description = "", int price = 0, ManufacturerType manufacturer = ManufacturerType.None)
		: this(name, description, price, manufacturer)
	{
		this.Location = location;
	}

	public SparePart(string name, string description = "", int price = 0, ManufacturerType manufacturer = ManufacturerType.None)
		: base(name, description, price, manufacturer)
	{
	}

	internal override void Register()
	{
		MainManager.Instance.SparePartManager.Register(this);
	}
}
