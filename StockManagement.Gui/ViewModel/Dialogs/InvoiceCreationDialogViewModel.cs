using System;
using System.Threading.Tasks;
using System.Windows;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;

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

		this.Invoice = invoice;
	}

	public Invoice Invoice
	{
		get { return _invoice; }
		set { this.SetField(ref _invoice, value); }
	}

	public bool Exists { get; set; }


	public static Task<InvoiceCreationDialogViewModel> CreateAsync(Invoice invoice, IInvoiceServiceProvider invoiceServiceProvider, IStockItemServiceProvider stockItemServiceProvider)
	{
		var ret = new InvoiceCreationDialogViewModel(invoice, invoiceServiceProvider, stockItemServiceProvider);
		return ret.InitializeAsync();
	}

	private async Task<InvoiceCreationDialogViewModel> InitializeAsync()
	{
		this.Exists = await _invoiceServiceProvider.GetInvoiceAync(this.Invoice.Number).ContinueWith(task => task.Result != null);
		return this;
	}

	public override async void Confirm()
	{
		try
		{
			await _invoiceServiceProvider.AddInvoiceAsync(this.Invoice);
			await this.Invoice.Items.UpdateStockItems(_stockItemServiceProvider);
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
