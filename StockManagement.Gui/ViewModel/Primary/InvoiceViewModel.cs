using System.Threading.Tasks;
using StockManagement.Kernel.Database;

namespace StockManagement.Gui.ViewModel.Primary;


public class InvoiceViewModel(IInvoiceServiceProvider invoiceServiceProvider) : ViewModelBase
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;


	public static Task<InvoiceViewModel> CreateAsync(IInvoiceServiceProvider invoiceServiceProvider)
	{
		var ret = new InvoiceViewModel(invoiceServiceProvider);
		return ret.InitializeAsync();
	}

	private async Task<InvoiceViewModel> InitializeAsync()
	{
		// These need to be assigned somewhere
		await _invoiceServiceProvider.GetInvoicesAsync();
		return this;
	}
}
