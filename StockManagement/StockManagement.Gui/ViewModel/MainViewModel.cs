using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private bool _showDialog = false;
	private Type _selectedStockItemType;

	private ObservableCollection<StockItem> _stockItems = new ObservableCollection<StockItem>();
	private object _stockItemsLock = new object();

	public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		ConfirmDialogCommand = new RelayCommand<string>(this.OnConfirmDialogCommand);
		CancelDialogCommand = new RelayCommand<string>(_ => this.ShowDialog = false);
		CreateStockItemCommand = new RelayCommand<string>(_ => this.ShowDialog = true);

		var kernelAssembly = Assembly.Load(new AssemblyName("Stockmanagement.Kernel"));
		StockItemTypes = ReflectionManager.GetTypesOfBase(kernelAssembly, typeof(StockItem));

		BindingOperations.EnableCollectionSynchronization(_stockItems, _stockItemsLock);
	}

	public RelayCommand<string> QuitCommand { get; }	
	public RelayCommand<string> ConfirmDialogCommand { get; }
	public RelayCommand<string> CancelDialogCommand { get; }
    public RelayCommand<string> CreateStockItemCommand { get; }

	public List<Type> StockItemTypes { get; }


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

	public string InputName { get; set; }

	public bool ShowDialog
	{
		get { return _showDialog; }
		private set { this.SetField(ref _showDialog, value); }
	}

	private StockItem? GetStockItemFromType()
	{
		if (this.SelectedStockItemType == typeof(Machine))
		{
			return new Machine()
			{
				Name = InputName,
				Description = "This a precise description of a machine"
			};
		}
		if (this.SelectedStockItemType == typeof(SparePart))
		{
			return new SparePart()
			{
				Name = InputName,
				Description = "This a precise description of a sparepart"
			};
		}
		if (this.SelectedStockItemType == typeof(Tire))
		{
			return new Tire()
			{
				Name = InputName,
				Description = "This a precise description of a tire"
			};
		}

		return null;
	}

	private void OnConfirmDialogCommand(string obj)
	{
		var item = this.GetStockItemFromType();
		if (item == null)
		{
			this.ShowDialog= false;
			MessageBox.Show("Failed to create Stock Item.");
			return;
		}

		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Value = item,
				Callback = success =>
				{
					if (success)
					{
						lock(_stockItemsLock)
						{
							this.StockItems.Add(item);
						}
					}
				}
			}
		};
		MainManagerFacade.PushCommand(command);
		this.ShowDialog = false;
	}
}
