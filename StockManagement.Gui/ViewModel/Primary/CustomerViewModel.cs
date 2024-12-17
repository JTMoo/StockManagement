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
using StockManagement.Gui.Commands;
using StockManagement.Gui.ViewModel.Dialogs;
using System.Windows;
using System.Diagnostics;

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
		this.CreateCustomerCommand = new RelayCommand<string>(this.OnCreateCustomerCommand);
		this.MoreInfoCommand = new RelayCommand<Customer>(this.OpenCustomerCreationDialogWithCustomer);

		_customerServiceProvider = customerServiceProvider;

		this.PropertyChanged += this.OnPropertyChangedEvent;
		this.SetupFilterConditions();
	}


	#region Properties
	public RelayCommand<string> CreateCustomerCommand { get; }
	public RelayCommand<Customer> MoreInfoCommand { get; }

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
		await this.UpdateCustomersAsync();
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

	private async void OnCreateCustomerCommand(string obj)
	{
		try
		{
			var newId = await GetNewCustomerId();
			this.OpenCustomerCreationDialogWithId(newId);
		}
		catch (Exception ex)
		{
			MessageBox.Show(Language.Resources.unexpectedError);
			Trace.WriteLine(ex.Message);
		}
	}

	private void OpenCustomerCreationDialogWithCustomer(Customer customer)
	{
		GuiManager.Instance.MainViewModel.Dialog = new CustomerCreationDialogViewModel(_customerServiceProvider, customer);
		GuiManager.Instance.MainViewModel.Dialog.DialogClosing += async _ => await this.UpdateCustomersAsync();
	}

	private void OpenCustomerCreationDialogWithId(int newId)
	{
		GuiManager.Instance.MainViewModel.Dialog = new CustomerCreationDialogViewModel(_customerServiceProvider, newId);
		GuiManager.Instance.MainViewModel.Dialog.DialogClosing += async _ => await this.UpdateCustomersAsync();
	}

	private async Task UpdateCustomersAsync()
	{
		var filteredCustomers = await _customerServiceProvider.GetCustomersAsync().ContinueWith(task => task.Result.Where(_filterFunctions));
		this.FilteredCustomers = new(filteredCustomers);
	}

	private async Task<int> GetNewCustomerId()
	{
		try
		{
			var customers = await _customerServiceProvider.GetCustomersAsync().ContinueWith(task => task.Result);
			return customers.Max(customer => customer.CustomerId) + 1;
		}
		catch (ArgumentNullException)
		{
			return 1;
		}
		catch (InvalidOperationException)
		{
			return 1;
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
			return string.IsNullOrEmpty(this.SearchCustomerIds) || !int.TryParse(this.SearchCustomerIds, out int customerId) || customer.CustomerId == customerId;
		});
	}
}
