using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System;
using System.Threading.Tasks;
using StockManagement.Gui.Commands;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Gui.ViewModel.Primary;


public class InvoiceViewModel : ViewModelBase
{
	private string _searchDates;
	private string _searchCustomers;
	private string _searchInvoiceNumber;
	private readonly List<Func<Invoice, bool>> _filterFunctions = [];
	private ObservableCollection<Invoice> _filteredInvoices;
	private readonly IInvoiceServiceProvider _invoiceServiceProvider;


	private InvoiceViewModel(IInvoiceServiceProvider invoiceServiceProvider)
	{
		_invoiceServiceProvider = invoiceServiceProvider;

		this.PropertyChanged += this.OnPropertyChangedEvent;
		this.SetupFilterConditions();
	}


	#region Properties
	public RelayCommand<Customer> MoreInfoCommand { get; }

	public ObservableCollection<Invoice> FilteredInvoices
	{
		get => _filteredInvoices;
		set => this.SetField(ref _filteredInvoices, value);
	}

	public string SearchCustomers
	{
		get { return _searchCustomers; }
		set { this.SetField(ref _searchCustomers, value); }
	}

	public string SearchDates
	{
		get { return _searchDates; }
		set { this.SetField(ref _searchDates, value); }
	}

	public string SearchInvoiceNumber
	{
		get { return _searchInvoiceNumber; }
		set { this.SetField(ref _searchInvoiceNumber, value); }
	}
	#endregion Properties


	public static Task<InvoiceViewModel> CreateAsync(IInvoiceServiceProvider invoiceServiceProvider)
	{
		var ret = new InvoiceViewModel(invoiceServiceProvider);
		return ret.InitializeAsync();
	}

	private async Task<InvoiceViewModel> InitializeAsync()
	{
		this.FilteredInvoices = new(await _invoiceServiceProvider.GetInvoicesAsync());
		return this;
	}

	private void OnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(this.SearchInvoiceNumber):
			case nameof(this.SearchCustomers):
			case nameof(this.SearchDates):
				this.OnRefreshSearch();
				break;
		}
	}

	private void OnRefreshSearch()
	{
		this.FilteredInvoices = new(this.FilteredInvoices.Where(_filterFunctions));
	}

	private void SetupFilterConditions()
	{
		_filterFunctions.Add(invoice =>
		{
			var fullNameMatch = string.Join(" ", invoice.Customer.Name, invoice.Customer.Lastname).StartsWith(this.SearchCustomers);
			return string.IsNullOrEmpty(this.SearchCustomers) || invoice.Customer.Name.StartsWith(this.SearchCustomers, StringComparison.CurrentCultureIgnoreCase) || invoice.Customer.Lastname.StartsWith(this.SearchCustomers, StringComparison.CurrentCultureIgnoreCase) || fullNameMatch;
		});
		_filterFunctions.Add(invoice =>
		{
			return string.IsNullOrEmpty(this.SearchDates) || invoice.Date.ToString("dd.MM.yyyy") == this.SearchDates;
		});
		_filterFunctions.Add(invoice =>
		{
			return string.IsNullOrEmpty(this.SearchInvoiceNumber) || !int.TryParse(this.SearchInvoiceNumber, out int invoiceNumber) || invoice.Number == invoiceNumber;
		});
	}
}
