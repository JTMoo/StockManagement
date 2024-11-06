using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using StockManagement.Gui.Commands;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class ShoppingCartDialogViewModel : DialogViewModelBase
{
	private long total;
	private string totalInWords;


	public ShoppingCartDialogViewModel(ObservableCollection<ShoppingCartItem> shoppingCartItems)
	{
		this.Items = shoppingCartItems;
		foreach (var item in this.Items)
		{
			item.PropertyChanged += this.OnShoppingCartItemChanged;
		}
		this.Items.CollectionChanged += this.OnItemsCollectionChanged;
		this.UpdateTotalPrice();

		this.IncreaseAmountCommand = new RelayCommand<ShoppingCartItem>(item => item.Amount += 1);
		this.DecreaseAmountCommand = new RelayCommand<ShoppingCartItem>(item => item.Amount -= 1);
		this.DeleteItemFromShoppingCartCommand = new RelayCommand<ShoppingCartItem>(this.OnDeleteItemFromShoppingCartCommand);
	}


	#region Properties
	public ObservableCollection<ShoppingCartItem> Items { get; }

	public long Total
	{
		get { return this.total; }
		set { this.SetField(ref this.total, value); }
	}
	public string TotalInWords
	{
		get { return this.totalInWords; }
		set { this.SetField(ref this.totalInWords, value); }
	}

	public RelayCommand<ShoppingCartItem> IncreaseAmountCommand { get; }
	public RelayCommand<ShoppingCartItem> DecreaseAmountCommand { get; }
	public RelayCommand<ShoppingCartItem> DeleteItemFromShoppingCartCommand { get; }
	#endregion Properties

	public override void Confirm(string param)
	{
		this.DeregisterFromEvents();
		base.Confirm(param);
	}

	public override void Cancel(string param)
	{
		this.DeregisterFromEvents();
		base.Cancel(param);
	}

	private void OnDeleteItemFromShoppingCartCommand(ShoppingCartItem item)
	{
		item.PropertyChanged -= this.OnShoppingCartItemChanged;
		this.Items.Remove(item);
	}

	private void OnShoppingCartItemChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.PropertyName) || sender is not ShoppingCartItem item) return;

		switch (e.PropertyName)
		{
			case nameof(item.Amount):
				this.UpdateTotalPrice();
				break;
		}
	}

	private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.UpdateTotalPrice();
	}

	private void DeregisterFromEvents()
	{
		foreach (var item in this.Items)
		{
			item.PropertyChanged -= this.OnShoppingCartItemChanged;
		}

		this.Items.CollectionChanged -= OnItemsCollectionChanged;
	}

	private void UpdateTotalPrice()
	{
		try
		{
			this.Total = this.Items.Sum(item => item.Amount * Convert.ToInt64(Math.Round(item.StockItem.Price, 0)));
		}
		catch (Exception e)
		{
			this.Total = 0;
			Trace.WriteLine(e.Message);
		}

		this.TotalInWords = ConversionHelper.ConvertToWords(this.Total);
	}
}
