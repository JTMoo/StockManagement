﻿using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private Type _selectedStockItemType;
	private ObservableCollection<StockItem> _stockItems = new ObservableCollection<StockItem>();
	private DialogViewModelBase? _dialog;

	private readonly object _stockItemsLock = new object();


	public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);

		BindingOperations.EnableCollectionSynchronization(_stockItems, _stockItemsLock);
	}

	#region Properties
	public RelayCommand<string> QuitCommand { get; }
    public RelayCommand<string> CreateStockItemCommand { get; }
	public DialogViewModelBase? Dialog
	{
		get { return _dialog; }
		private set { this.SetField(ref _dialog, value); }
	}
	public List<Type> StockItemTypes { get; internal set; }

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

	internal void StockItemCreationFinished(bool success, StockItem stockItem)
	{
		if (!success)
		{
			Trace.WriteLine("Unsuccessfully tried to add: " + stockItem.Name);
			return;
		}

		lock (_stockItemsLock)
		{
			this.StockItems.Add(stockItem);
		}
	}

	private void OnCreateStockItemCommand(string param)
	{
		this.Dialog = GuiManager.Instance.StockItemToViewModel[this.SelectedStockItemType];
		this.Dialog.DialogClosing += this.OnDialogClosing;
	}

	private void OnDialogClosing(bool success)
	{
		if (this.Dialog == null) return;

		this.Dialog.DialogClosing -= this.OnDialogClosing;
		this.Dialog = null;
	}
}