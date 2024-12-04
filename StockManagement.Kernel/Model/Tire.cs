using System.ComponentModel.DataAnnotations;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.tire))]
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


	public override string ToString()
	{
		return $"{Language.Resources.tire}: {this.Name}, {this.Manufacturer}, {this.Amount}, {this.Location}, {this.Dimensions}";
	}

	internal override void Register()
	{
		MainManager.Instance.TireManager.Register(this);
	}

	internal override void Deregister()
	{
		MainManager.Instance.TireManager.Deregister(this);
	}

	internal override void Update(Action callback)
	{
		MainManager.Instance.TireManager.Update(this, callback);
	}
}