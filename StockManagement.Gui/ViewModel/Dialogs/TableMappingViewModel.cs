using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Commands.StockItemCommands;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui.ViewModel.Dialogs;


public partial class TableMappingViewModel : DialogViewModelBase
{
	private readonly Dictionary<PropertyInfo, string> tableNamesToProperties = [];
	private Type selectedStockItemType;
	private readonly IXLWorksheet worksheet;


	public TableMappingViewModel(IXLWorksheet worksheet)
	{
		this.worksheet = worksheet;
		this.GetTableDataFromWorksheet();

		this.SelectedItemChangedCommand = new RelayCommand<object>(this.OnSelectedItemChangedCommand);

		this.PropertyChanged += OnPropertyChangedEvent;
	}

	#region Properties
	public List<Type> StockItemTypes { get; } = new(GuiManager.Instance.StockItemTypes);
	public ObservableCollection<PropertyInfo> SelectedStockItemTypeProperties { get; } = [];
	public ObservableCollection<string> TableNames { get; } = [];

	public Type SelectedStockItemType
	{
		get { return this.selectedStockItemType; }
		set { this.SetField(ref this.selectedStockItemType, value); }
	}

	public RelayCommand<object> SelectedItemChangedCommand { get; }
	#endregion Properties

	public override void Confirm(string param)
	{
		GuiManager.Instance.ShowWaitDialog();

		try
		{
			var stockItems = this.ExtractStockItemsFromExcelSheet();
			var command = new StockItemCreationCommand
			{
				Data = new StockItemCommandData()
				{
					DataToRegister = stockItems,
					Type = StockItemCommandData.CreationCommandType.Multiple,
					Callback = _ => GuiManager.Instance.HideWaitDialog()
				}
			};
			MainManagerFacade.PushCommand(command);
		}
		catch(FailedConversionException ex)
		{
			MessageBox.Show(ex.Message);
			GuiManager.Instance.HideWaitDialog();
		}

		base.Confirm(param);
	}

	private List<StockItem> ExtractStockItemsFromExcelSheet()
	{
		if (this.worksheet.FirstRowUsed() is not IXLRow row)
		{
			Trace.WriteLine($"First row could not be found in {this.worksheet.Name}");
			return [];
		}

		var headerRowRange = row.RowUsed();
		var matchingActions = CreatePropertyMatchingActionsFromTableHeaders(headerRowRange);

		var currentRow = headerRowRange.RowBelow();
		var stockItems = new List<StockItem>();
		while (!currentRow.IsEmpty())
		{
			if (Activator.CreateInstance(this.SelectedStockItemType) is not StockItem stockItem) continue;

			matchingActions.ForEach(action => action(currentRow, stockItem));
			stockItems.Add(stockItem);

			currentRow = currentRow.RowBelow();
		}

		return stockItems;
	}

	private List<Action<IXLRangeRow, StockItem>> CreatePropertyMatchingActionsFromTableHeaders(IXLRangeRow headerRowRange)
	{
		List<Action<IXLRangeRow, StockItem>> matchingActions = [];
		foreach (var pair in this.tableNamesToProperties)
		{
			if (!TryGetColumnNumberFrom(pair.Value, headerRowRange, out int columnNumber)) continue;
			matchingActions.Add((row, stockItem) =>
			{
				try
				{
					if (row.Cell(columnNumber).GetString() is not string cellValue || string.IsNullOrEmpty(cellValue)) return;
					if (TypeDescriptor.GetConverter(pair.Key.PropertyType).ConvertFromString(cellValue) is not object propertyValue) return;
					pair.Key.SetValue(stockItem, propertyValue);
				}
				catch (ArgumentException argEx)
				{
					throw new FailedConversionException(string.Concat(string.Format(Language.Resources.failedConversion, argEx.ParamName, columnNumber), $" ({argEx.Message})"));
				}
				catch(Exception ex)
				{
					Trace.WriteLine($"Exception durch matchingActions: {ex.Message}");
					return;
				}
			});
		}

		return matchingActions;
	}

	private void GetTableDataFromWorksheet()
	{
		if (this.worksheet.FirstRowUsed() is not IXLRow row)
		{
			Trace.WriteLine($"First Row used could not be found in {this.worksheet.Name}");
			return;
		}

		foreach (var cell in row.CellsUsed())
		{
			this.TableNames.Add(cell.GetString().ReplaceLineBreakWithWhitespace());
		}
	}

	private void OnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(this.SelectedStockItemType)) return;

		this.SelectedStockItemTypeProperties.EqualizeTo(this.SelectedStockItemType.GetProperties());
		tableNamesToProperties.Clear();
	}

	private void OnSelectedItemChangedCommand(object arg)
	{
		if (arg is not object[] args || args.Length != 2) return;
		if (args[0] is not PropertyInfo info) return;
		if (args[1] is not string selectedTableName) return;

		this.tableNamesToProperties[info] = selectedTableName;
	}

	private static bool TryGetColumnNumberFrom(string value, IXLRangeRow headerRowRange, out int columnNumber)
	{
		columnNumber = -1;

		foreach (var cell in headerRowRange.Cells())
		{
			columnNumber++;
			if(cell.GetString().ReplaceLineBreakWithWhitespace().Equals(value, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}
}
