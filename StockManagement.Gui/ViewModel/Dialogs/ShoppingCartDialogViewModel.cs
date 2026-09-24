using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StockManagement.Gui.Commands;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class ShoppingCartDialogViewModel : DialogViewModelBase
{
	private long total;
	private string totalInWords;
	private Customer _selectedCustomer;
	private readonly ICustomerServiceProvider _customerServiceProvider;
	private readonly ISaleService _saleService;


	public ShoppingCartDialogViewModel(IEnumerable<ShoppingCartItem> shoppingCartItems, ICustomerServiceProvider customerServiceProvider, ISaleService saleService)
	{
		_customerServiceProvider = customerServiceProvider;
		_saleService = saleService;

		this.Items = new(shoppingCartItems);
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
	public ObservableCollection<Customer> AvailableCustomers { get; private set; }

	public Customer SelectedCustomer
	{
		get { return _selectedCustomer; }
		set { this.SetField(ref this._selectedCustomer, value); }
	}
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


	public override async void Confirm()
	{
		var invoice = _saleService.CreateInvoice(this.SelectedCustomer, this.Items, DateTime.Now);
		this.DeregisterFromEvents();
		GuiManager.Instance.MainViewModel.Dialog = await InvoiceCreationDialogViewModel.CreateAsync(invoice, _saleService);
	}

	public override void Cancel()
	{
		this.DeregisterFromEvents();
		base.Cancel();
	}
	public static Task<ShoppingCartDialogViewModel> CreateAsync(IEnumerable<ShoppingCartItem> shoppingCartItems, ICustomerServiceProvider customerServiceProvider, ISaleService saleService)
	{
		var ret = new ShoppingCartDialogViewModel(shoppingCartItems, customerServiceProvider, saleService);
		return ret.InitializeAsync();
	}

	private async Task<ShoppingCartDialogViewModel> InitializeAsync()
	{
		this.AvailableCustomers = new(await _customerServiceProvider.GetCustomersAsync());
		return this;
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
			this.Total = _saleService.CalculateTotal(this.Items);
		}
		catch (Exception e)
		{
			this.Total = 0;
			Trace.WriteLine(e.Message);
		}

		this.TotalInWords = ConversionHelper.ConvertToWords(this.Total);
	}
}
