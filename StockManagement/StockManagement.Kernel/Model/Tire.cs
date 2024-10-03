using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Tire : StockItem
{
	public Tire() : base()
	{
	}

	public Tire (double rimDiameter, double profile, double width, string name, string code = "", string description = "", int amount = 1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None) 
		: base (name:name, code:code, description:description, amount:amount, price:price, manufacturer:manufacturer)
	{
		this.Dimensions = new TireDimensions(rimDiameter, profile, width);
	}

	public TireDimensions Dimensions { get; set; }

	internal override void Register()
	{
		MainManager.Instance.TireManager.Register(this);
	}

	internal override void Update(Action callback)
	{
		MainManager.Instance.TireManager.Update(this, callback);
	}
}