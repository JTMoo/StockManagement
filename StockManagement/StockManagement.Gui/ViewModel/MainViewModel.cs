using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private Type _selectedStockItemType;
	private ObservableCollection<StockItem> _stockItems = new ObservableCollection<StockItem>();
	private Dictionary<Type, DialogViewModelBase> _itemTypeToViewModel = new Dictionary<Type, DialogViewModelBase>();


	public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);

		this.AssignDialogs();

		BindingOperations.EnableCollectionSynchronization(_stockItems, this.StockItemsLock);
	}


	public RelayCommand<string> QuitCommand { get; }
    public RelayCommand<string> CreateStockItemCommand { get; }
	public DialogViewModelBase Dialog { get; set; }
	public List<Type> StockItemTypes { get; private set; }
	public object StockItemsLock { get; } = new object();

	public ObservableCollection<StockItem> StockItems
	{
		get { return _stockItems; }
		internal set { this.SetField(ref _stockItems, value); }
	}

	public Type SelectedStockItemType
	{
		get { return _selectedStockItemType; }
		set { this.SetField(ref _selectedStockItemType, value); }
	}

	private void AssignDialogs()
	{
		var kernelAssembly = Assembly.Load(new AssemblyName("Stockmanagement.Kernel"));
		this.StockItemTypes = ReflectionManager.GetTypesOfBase(kernelAssembly, typeof(StockItem));

		var guiAssembly = Assembly.GetExecutingAssembly();
		var viewModels = ReflectionManager.GetTypesInNamespace(guiAssembly, "StockManagement.Gui.ViewModel.StockItemCreation");

		if (this.StockItemTypes.Count != viewModels.Count)
		{
			Trace.WriteLine("Amount of StockItems and CreationDialog-VMs do not match.");
			return;
		}

		for (int i = 0; i < this.StockItemTypes.Count; i++)
		{
			var vm = Activator.CreateInstance(viewModels[i]) as DialogViewModelBase;
			if (vm == null)
			{
				Trace.WriteLine("Failed to create instance of viewmodel.");
				return;
			}
			this._itemTypeToViewModel[this.StockItemTypes[i]] = vm;
		}
	}

	private void OnCreateStockItemCommand(string param)
	{
		this.Dialog = _itemTypeToViewModel[this.SelectedStockItemType];
		this.Dialog.Open();
		this.Dialog.DialogClosing += this.OnDialogClosing;
	}

	private void OnDialogClosing(bool success)
	{
		this.Dialog.DialogClosing -= this.OnDialogClosing;
		this.Dialog = new DialogViewModelBase();
	}
}
