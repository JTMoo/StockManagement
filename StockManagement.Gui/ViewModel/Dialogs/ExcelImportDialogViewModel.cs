using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ClosedXML.Excel;
using StockManagement.Import.Core.Contracts;

namespace StockManagement.Gui.ViewModel.Dialogs;


public sealed class ExcelImportDialogViewModel : DialogViewModelBase
{
	private string selectedWorksheetName = string.Empty;
	private XLWorkbook workbook;
	private readonly IStockItemImportService _stockItemImportService;


	private ExcelImportDialogViewModel(IStockItemImportService stockItemImportService)
	{
		_stockItemImportService = stockItemImportService;
	}

	#region Properties
	public ObservableCollection<string> WorksheetNames { get; } = [];

	public string SelectedWorksheetName
	{
		get { return this.selectedWorksheetName; }
		set { this.SetField(ref this.selectedWorksheetName, value); }
	}
	#endregion Properties

	public override void Confirm()
	{
		GuiManager.Instance.MainViewModel.Dialog = new TableMappingViewModel(this.workbook.Worksheet(this.SelectedWorksheetName), _stockItemImportService);
	}

	public static Task<ExcelImportDialogViewModel> CreateAsync(string filePath, IStockItemImportService stockItemImportService)
	{
		var ret = new ExcelImportDialogViewModel(stockItemImportService);
		return ret.InitializeAsync(filePath);
	}

	private async Task<ExcelImportDialogViewModel> InitializeAsync(string filePath)
	{
		await this.GetExcelSheetNames(filePath);
		return this;
	}

	private Task GetExcelSheetNames(string filePath)
	{
		return Task.Run(() =>
		{
			this.workbook = new XLWorkbook(filePath);

			foreach (var sheet in this.workbook.Worksheets)
			{
				this.WorksheetNames.Add(sheet.Name);
			}
		});
	}
}
