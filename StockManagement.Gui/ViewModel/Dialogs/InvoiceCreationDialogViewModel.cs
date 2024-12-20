using System.Threading.Tasks;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

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

	public override async void Confirm(string obj)
	{
		await _invoiceServiceProvider.AddInvoiceAsync(this.Invoice);
		//await _stockItemServiceProvider.UpdateStockItemsAsync(this.Invoice.Items);
		base.Confirm(obj);
	}
}
