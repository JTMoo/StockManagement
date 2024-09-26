using System;
using System.Collections.Generic;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class StockItemTypeSelectionDialogViewModel(List<Type> stockItemTypes) : DialogViewModelBase
{
	private Type _selectedStockItemType;

	#region Properties
	public Type SelectedStockItemType
	{
		get { return _selectedStockItemType; }
		set { this.SetField(ref _selectedStockItemType, value); }
	}

	public List<Type> StockItemTypes { get; } = stockItemTypes;
	#endregion Properties

	public override void Confirm(string obj)
	{
		GuiManager.Instance.MainViewModel.Dialog = GuiManager.Instance.StockItemToViewModel[this.SelectedStockItemType];
	}
}