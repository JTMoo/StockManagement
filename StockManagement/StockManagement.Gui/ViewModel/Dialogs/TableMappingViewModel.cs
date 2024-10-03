using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using StockManagement.Gui.Commands;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui.ViewModel.Dialogs;


public partial class TableMappingViewModel : DialogViewModelBase
{
	private Type selectedStockItemType;
	private readonly IXLWorksheet worksheet;


	public TableMappingViewModel(IXLWorksheet worksheet)
	{
		this.worksheet = worksheet;
		this.GetTableDataFromWorksheet();

		this.SelectedItemChangedCommand = new RelayCommand<PropertyInfo>(this.OnSelectedItemChangedCommand);

		this.PropertyChanged += OnPropertyChangedEvent;
	}

	#region Properties
	public List<Type> StockItemTypes { get; } = new(GuiManager.Instance.MainViewModel.StockItemTypes);
	public ObservableCollection<PropertyInfo> SelectedStockItemTypeProperties { get; } = [];
	public ObservableCollection<string> TableNames { get; } = [];

	public Type SelectedStockItemType
	{
		get { return this.selectedStockItemType; }
		set { this.SetField(ref this.selectedStockItemType, value); }
	}

	public RelayCommand<PropertyInfo> SelectedItemChangedCommand { get; }
	#endregion Properties

	private void GetTableDataFromWorksheet()
	{
		if (TryGetTableHeadersFromTables()) return;

		foreach (var cell in this.worksheet.FirstRowUsed().CellsUsed())
		{
			this.TableNames.Add(LineBreakDetection().Replace(cell.GetString(), " "));
		}
	}

	private bool TryGetTableHeadersFromTables()
	{
		var tableAmount = this.worksheet.Tables.Count();
		if (tableAmount > 1 || tableAmount <= 0)
		{
			return false;
		}

		var table = this.worksheet.Tables.Table(0);
		foreach (var cell in table.FirstRowUsed().CellsUsed())
		{
			this.TableNames.Add(LineBreakDetection().Replace(cell.GetString(), " "));
		}

		return true;
	}

	private void OnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(this.SelectedStockItemType)) return;

		this.SelectedStockItemTypeProperties.EqualizeTo(this.SelectedStockItemType.GetProperties());
	}

	private void OnSelectedItemChangedCommand(PropertyInfo info)
	{
		Trace.WriteLine("Works so far");
	}

	[GeneratedRegex(@"\t|\n|\r")]
	private static partial Regex LineBreakDetection();
}
