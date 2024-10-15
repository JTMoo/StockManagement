using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands.StockItemCommands;


public class StockItemChangeAmountCommand : ICommand
{
	public CommandData Data { get; set; }

	public ChangeType SelectedChangeType { get; set; }

	public bool Execute()
	{
		if (Data == null) return false;
		if (Data is not StockItemCommandData) return false;
		if (Data.Value is not int) return false;

		var amount = (int)Data.Value;
		var stockItem = ((StockItemCommandData)Data).DataToRegister;
		if (this.SelectedChangeType == ChangeType.Checkout)
		{
			amount *= -1;
		}

		var transaction = new Transaction(stockItem.Name, DateTime.Now, Transaction.Kind.Amount, amount);
		stockItem.Amount += amount;
		stockItem.Update(() => OnUpdateCompleted(transaction));

		return false;
	}

	/// ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Callback that is executed after the update of the stock item worked.
	/// This makes sure, that the transaction is only saved, if the update was successful.
	/// If the update fails, it results in an exception and this callback is ignored.
	/// </summary>
	/// ----------------------------------------------------------------------------------------------
	/// <param name="transaction">Transaction that documents change to stock item</param>
	/// ----------------------------------------------------------------------------------------------
	private static void OnUpdateCompleted(Transaction transaction)
	{
		if (transaction == null) return;

		Database.TransactionDataAccess.Add(transaction);
	}

	public enum ChangeType
	{
		Checkout = 0,

		Checkin
	}
}
