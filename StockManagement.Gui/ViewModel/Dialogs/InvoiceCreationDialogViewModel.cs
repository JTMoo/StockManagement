using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using StockManagement.Gui.Commands;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Gui.ViewModel.Dialogs;


internal class InvoiceCreationDialogViewModel : DialogViewModelBase
{
	private Invoice _invoice;
	private readonly ISaleService? _saleService;


	private InvoiceCreationDialogViewModel(Invoice invoice)
	{
		this.Invoice = invoice;
	}

	private InvoiceCreationDialogViewModel(Invoice invoice, ISaleService saleService)
	{
		_saleService = saleService;

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


	/// <summary>
	/// Shows an invoice that is already stored
	/// </summary>
	public static Task<InvoiceCreationDialogViewModel> CreateAsync(Invoice invoice)
	{
		var ret = new InvoiceCreationDialogViewModel(invoice);
		return ret.InitializeAsync();
	}

	/// <summary>
	/// Shows a new invoice that completes the sale on confirm
	/// </summary>
	public static Task<InvoiceCreationDialogViewModel> CreateAsync(Invoice invoice, ISaleService saleService)
	{
		var ret = new InvoiceCreationDialogViewModel(invoice, saleService);
		return ret.InitializeAsync();
	}

	private async Task<InvoiceCreationDialogViewModel> InitializeAsync()
	{
		if (this.Invoice.Number != 0 || _saleService == null)
		{
			this.Exists = true;
			return this;
		}

		this.Invoice.Number = await _saleService.GetNextInvoiceNumberAsync();
		return this;
	}

	public override async void Confirm()
	{
		if (this.Exists || _saleService == null)
		{
			base.Confirm();
			return;
		}

		try
		{
			var result = await _saleService.CompleteSaleAsync(this.Invoice);
			if (!result.Succeeded)
			{
				var message = string.Join(" ", Language.Resources.exceptionShoppingCartItemOutOfRange, $"({string.Join(", ", result.UnavailableItems)})");
				MessageBox.Show(message, Language.Resources.invoices, MessageBoxButton.OK, MessageBoxImage.Error);
				this.Cancel();
				return;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(Language.Resources.unexpectedError, Language.Resources.invoices, MessageBoxButton.OK, MessageBoxImage.Error);
			Trace.WriteLine(ex);
			this.Cancel();
			return;
		}

		base.Confirm();
	}
}
