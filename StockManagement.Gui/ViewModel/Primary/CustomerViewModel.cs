using StockManagement.Kernel.Model;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections.ObjectModel;
using StockManagement.Kernel.Database;
using System.Threading.Tasks;
using System.Linq;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui.ViewModel.Primary;


public class CustomerViewModel : ViewModelBase
{
	private readonly List<Func<Customer, bool>> _filterFunctions = [];
	private string _searchNames;
	private string _searchCustomerIds;
	private string _searchLastnames;
	private ObservableCollection<Customer> _filteredCustomers;
	private readonly ICustomerServiceProvider _customerServiceProvider;


	private CustomerViewModel(ICustomerServiceProvider customerServiceProvider)
	{
		_customerServiceProvider = customerServiceProvider;

		this.PropertyChanged += this.OnPropertyChangedEvent;
		this.SetupFilterConditions();
	}


	#region Properties
	public ObservableCollection<Customer> FilteredCustomers
	{
		get => this._filteredCustomers;
		set => this.SetField(ref this._filteredCustomers, value);
	}

	public string SearchNames
	{
		get { return _searchNames; }
		set { this.SetField(ref _searchNames, value); }
	}

	public string SearchLastnames
	{
		get { return _searchLastnames; }
		set { this.SetField(ref _searchLastnames, value); }
	}

	public string SearchCustomerIds
	{
		get { return _searchCustomerIds; }
		set { this.SetField(ref _searchCustomerIds, value); }
	}
	#endregion Properties


	public static Task<CustomerViewModel> CreateAsync(ICustomerServiceProvider customerServiceProvider)
	{
		var ret = new CustomerViewModel(customerServiceProvider);
		return ret.InitializeAsync();
	}

	private async Task<CustomerViewModel> InitializeAsync()
	{
		this.FilteredCustomers = new(await _customerServiceProvider.GetCustomersAsync());
		return this;
	}


	private void OnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(this.SearchCustomerIds):
			case nameof(this.SearchNames):
			case nameof(this.SearchLastnames):
				this.OnRefreshSearch();
				break;
		}
	}
	private void OnRefreshSearch()
	{
		this.FilteredCustomers = new(this.FilteredCustomers.Where(_filterFunctions));
	}

	private void SetupFilterConditions()
	{
		_filterFunctions.Add(customer =>
		{
			return string.IsNullOrEmpty(this.SearchNames) || Regex.IsMatch(customer.Name.ToLower(), this.SearchNames.ToLower());
		});
		_filterFunctions.Add(customer =>
		{
			return string.IsNullOrEmpty(this.SearchLastnames) || Regex.IsMatch(customer.Lastname.ToLower(), this.SearchLastnames.ToLower());
		});
		_filterFunctions.Add(customer =>
		{
			return string.IsNullOrEmpty(this.SearchCustomerIds) || Regex.IsMatch(customer.CustomerId.ToLower(), this.SearchCustomerIds.ToLower());
		});
	}
}
