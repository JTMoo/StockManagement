using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private Type _selectedStockItemType;
	private ObservableCollection<StockItem> _stockItems = new ObservableCollection<StockItem>();


	public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);

		BindingOperations.EnableCollectionSynchronization(_stockItems, this.StockItemsLock);
	}

	#region Properties
	public RelayCommand<string> QuitCommand { get; }
    public RelayCommand<string> CreateStockItemCommand { get; }
	public DialogViewModelBase Dialog { get; set; }
	public List<Type> StockItemTypes { get; internal set; }
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
	#endregion Properties

	private void OnCreateStockItemCommand(string param)
	{
		this.Dialog = GuiManager.Instance.StockItemToViewModel[this.SelectedStockItemType];
		this.Dialog.Open();
		this.Dialog.DialogClosing += this.OnDialogClosing;
	}

	private void OnDialogClosing(bool success)
	{
		this.Dialog.DialogClosing -= this.OnDialogClosing;
		this.Dialog = new DialogViewModelBase();
	}
}
