using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands;


public class StockItemCommand : ICommand
{
	public CommandData Data { get; set; }

	public bool Execute()
	{
		if (Data == null) return false;

		switch (Data.Type)
		{
			case Type SparePart:
				MainManager.Instance.SparePartManager.AddSparePart(new SparePart());
				break;
		}

		return true;
	}

	public enum Type
	{
		Machine,

		SparePart,

		Tire
	}
}
