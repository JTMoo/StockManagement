using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;
using StockManagement.Gui.Commands;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui.ViewModel.Dialogs;


public partial class TableMappingViewModel : DialogViewModelBase
{
	private readonly Dictionary<PropertyInfo, string> _tableHeaderPropertyPairs = [];
	private readonly IXLWorksheet _worksheet;
	private readonly IStockItemServiceProvider _stockItemServiceProvider;


	public TableMappingViewModel(IXLWorksheet worksheet, IStockItemServiceProvider stockItemServiceProvider)
	{
		_stockItemServiceProvider = stockItemServiceProvider;
		_worksheet = worksheet;

		this.GetTableDataFromWorksheet();

		this.SelectedItemChangedCommand = new RelayCommand<object>(this.OnSelectedItemChangedCommand);

		this.StockItemProperties.EqualizeTo(typeof(StockItem).GetProperties());
	}

	#region Properties
	public ObservableCollection<PropertyInfo> StockItemProperties { get; } = [];
	public ObservableCollection<string> TableHeaders { get; } = [];

	public RelayCommand<object> SelectedItemChangedCommand { get; }
	#endregion Properties

	public override async void Confirm()
	{
		GuiManager.Instance.ShowWaitDialog();

		var items = await Task.Run(this.ExtractStockItemsFromExcelSheet).ContinueWith(task => task.Result.ToList());
		var existingItems = await _stockItemServiceProvider.GetAllStockItemsAsync().ContinueWith(task => task.Result.ToList());
		var dupCount = items.RemoveAndCountDuplicates(existingItems);

		var result = MessageBox.Show($"Extracted {items.Count + dupCount} elements. {dupCount} are duplicates. Do you want to complete the import with the remaining {items.Count} elements?", Language.Resources.excelImport, MessageBoxButton.YesNo);
		if (result != MessageBoxResult.Yes)
		{
			GuiManager.Instance.HideWaitDialog();
			this.Close(false);
			return;
		}

		await _stockItemServiceProvider.AddManyStockItemsAsync(items);

		GuiManager.Instance.HideWaitDialog();
		base.Confirm();
	}

	private IList<StockItem> ExtractStockItemsFromExcelSheet()
	{
		if (this._worksheet.FirstRowUsed() is not IXLRow row)
		{
			Trace.WriteLine($"First row could not be found in {this._worksheet.Name}");
			return [];
		}

		var headerRowRange = row.RowUsed();
		var matchingActions = CreateTableHeaderToPropertyMatchingActions(headerRowRange);
		return ExtractStockItemsFromExcelRows(matchingActions, firstDataRow: headerRowRange.RowBelow());
	}

	private static IList<StockItem> ExtractStockItemsFromExcelRows(IList<Action<IXLRangeRow, StockItem>> matchingActions, IXLRangeRow firstDataRow)
	{
		var currentRow = firstDataRow;
		IList<StockItem> items = [];
		while (!currentRow.IsEmpty())
		{
			try
			{
				items.Add(CreateStockItem(matchingActions, currentRow));
			}
			catch (FailedExcelConversionException)
			{
			}

			currentRow = currentRow.RowBelow();
		}

		return items;
	}

	private static StockItem CreateStockItem(IList<Action<IXLRangeRow, StockItem>> matchingActions, IXLRangeRow currentRow)
	{
		var stockItem = new StockItem();
		matchingActions.ToList().ForEach(action => action(currentRow, stockItem));
		return stockItem;
	}

	private IList<Action<IXLRangeRow, StockItem>> CreateTableHeaderToPropertyMatchingActions(IXLRangeRow headerRowRange)
	{
		IList<Action<IXLRangeRow, StockItem>> matchingActions = [];
		foreach (var tableHeaderPropertyPair in _tableHeaderPropertyPairs)
		{
			if (!TryGetColumnNumberFromCellValue(tableHeaderPropertyPair.Value, headerRowRange, out int columnNumber)) continue;

			matchingActions.Add(CreateTableHeaderToPropertyMatchingAction(tableHeaderPropertyPair, columnNumber));
		}

		return matchingActions;
	}

	private static Action<IXLRangeRow, StockItem> CreateTableHeaderToPropertyMatchingAction(KeyValuePair<PropertyInfo, string> tableHeaderPropertyPair, int columnNumber)
	{
		return (row, stockItem) =>
		{
			try
			{
				SetStockItemFromExcelRow(row, stockItem, tableHeaderPropertyPair, columnNumber);
			}
			catch (ArgumentException argEx)
			{
				throw new FailedExcelConversionException(argEx, columnNumber);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Exception durch matchingActions: {ex.Message}");
				return;
			}
		};
	}

	private static void SetStockItemFromExcelRow(IXLRangeRow row, StockItem stockItem, KeyValuePair<PropertyInfo, string> tableHeaderPropertyPair, int columnNumber)
	{
		if (!row.Cell(columnNumber).TryGetValue(out string cellValue)) return;
		if (!tableHeaderPropertyPair.Key.PropertyType.TryConvertFromString(cellValue, out object propertyValue)) return;

		tableHeaderPropertyPair.Key.SetValue(stockItem, propertyValue);
	}

	private void GetTableDataFromWorksheet()
	{
		if (this._worksheet.FirstRowUsed() is not IXLRow row)
		{
			Trace.WriteLine($"First Row used could not be found in {this._worksheet.Name}");
			return;
		}

		foreach (var cell in row.CellsUsed())
		{
			this.TableHeaders.Add(cell.GetString().ReplaceLineBreakWithWhitespace());
		}
	}

	private void OnSelectedItemChangedCommand(object arg)
	{
		if (arg is not object[] args || args.Length != 2) return;
		if (args[0] is not PropertyInfo info) return;
		if (args[1] is not string selectedTableName) return;

		_tableHeaderPropertyPairs[info] = selectedTableName;
	}

	private static bool TryGetColumnNumberFromCellValue(string value, IXLRangeRow headerRowRange, out int columnNumber)
	{
		columnNumber = 0;
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
