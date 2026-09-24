using System.Collections.Generic;
using System.Threading.Tasks;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class StockItemCreationDialogViewModel(IStockItemServiceProvider stockItemServiceProvider, StockItem stockItem, IEnumerable<string> manufacturers) : DialogViewModelBase()
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	#region Properties
	public StockItem StockItem { get; set; } = stockItem;

	/// <summary>
	/// Manufacturers to pick from; a new name can be typed too.
	/// </summary>
	public IEnumerable<string> Manufacturers { get; } = manufacturers;
	#endregion

	public override async void Confirm()
	{
		await CreateOrUpdateStockItem();
		base.Confirm();
	}

	private async Task CreateOrUpdateStockItem()
	{
		var stockItem = await _stockItemServiceProvider.GetStockItemAsync(this.StockItem.Code);
		if (stockItem == null)
		{
			await _stockItemServiceProvider.AddStockItemAsync(this.StockItem);
		}
		else
		{
			var result = await _stockItemServiceProvider.UpdateStockItemAsync(this.StockItem).ContinueWith(task => task.Result.ModifiedCount == 1);
		}
	}
}