using System.ComponentModel.DataAnnotations;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.machine))]
public class Machine : StockItem
{
	public Machine() : base()
	{
	}

	public Machine(string name, string description = "", int amount = 1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None)
		: base(name: name, description:description, amount:amount, price:price, manufacturer:manufacturer)
	{
	}

	public override string ToString()
	{
		return $"{Language.Resources.machine}: {this.Name}, {this.Manufacturer}, {this.Amount}, {this.Location}";
	}

	internal override void Register()
	{
		MainManager.Instance.MachineManager.Register(this);
	}

	internal override void Deregister()
	{
		MainManager.Instance.MachineManager.Deregister(this);
	}

	internal override void Update(Action callback)
	{
		MainManager.Instance.MachineManager.Update(this, callback);
	}
}