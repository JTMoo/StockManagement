using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands.StockItemCommands;


public class StockItemChangeAmountCommand : ICommand
{
	public CommandData Data { get; set; }

	public ChangeType SelectedChangeType { get; set; }

	public bool Execute()
	{
		if (Data == null) return false;
		if (Data is not StockItemCommandData data || data.DataToRegister is not StockItem stockItem) return false;
		if (Data.Value is not int amount) return false;

		if (this.SelectedChangeType == ChangeType.Checkout)
		{
			amount *= -1;
		}

		var transaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, amount);
		stockItem.Amount += amount;
		stockItem.Update(() => DatabaseManager.Add<Transaction>(transaction));

		return false;
	}

	public enum ChangeType
	{
		Checkout = 0,

		Checkin
	}
}
