using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using StockManagement.Gui.Commands;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.Dialogs;


internal class InvoiceCreationDialogViewModel : DialogViewModelBase
{
	private Invoice _invoice;
	private readonly IInvoiceServiceProvider _invoiceServiceProvider;
	private readonly IStockItemServiceProvider _stockItemServiceProvider;


	private InvoiceCreationDialogViewModel(Invoice invoice, IInvoiceServiceProvider invoiceServiceProvider, IStockItemServiceProvider stockItemServiceProvider)
	{
		_invoiceServiceProvider = invoiceServiceProvider;
		_stockItemServiceProvider = stockItemServiceProvider;

		this.ChangeSaleConditionCommand = new RelayCommand<SaleCondition>(condition => this.Invoice.SaleCondition = condition);
		this.Invoice = invoice;
	}

	public Invoice Invoice
	{
		get { return _invoice; }
		set { this.SetField(ref _invoice, value); }
	}

	public RelayCommand<SaleCondition> ChangeSaleConditionCommand { get; }
	public bool Exists { get; set; }


	public static Task<InvoiceCreationDialogViewModel> CreateAsync(Invoice invoice, IInvoiceServiceProvider invoiceServiceProvider, IStockItemServiceProvider stockItemServiceProvider)
	{
		var ret = new InvoiceCreationDialogViewModel(invoice, invoiceServiceProvider, stockItemServiceProvider);
		return ret.InitializeAsync();
	}

	private async Task<InvoiceCreationDialogViewModel> InitializeAsync()
	{
		if (this.Invoice.Number != 0)
		{
			this.Exists = true;
		}

		var invoices = await _invoiceServiceProvider.GetInvoicesAsync();
		this.Invoice.Number = !invoices.Any() ? 1 : invoices.Max(invoice => invoice.Number) + 1;
		return this;
	}

	public override async void Confirm()
	{
		try
		{
			await this.Invoice.Items.UpdateStockItems(_stockItemServiceProvider);
			await _invoiceServiceProvider.AddInvoiceAsync(this.Invoice);
		}
		catch (ArgumentOutOfRangeException ex)
		{
			var message = string.Join(" ", ex.Message, $"({ex.ParamName})");
			MessageBox.Show(message, Language.Resources.invoices, MessageBoxButton.OK, MessageBoxImage.Error);
			this.Cancel();
		}

		base.Confirm();
	}
}
