using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ClosedXML.Excel;
using SharpCompress.Common;

namespace StockManagement.Gui.ViewModel.Dialogs;


public sealed class ExcelImportDialogViewModel : DialogViewModelBase
{
	private string selectedWorksheetName = string.Empty;
	private XLWorkbook workbook;


	private ExcelImportDialogViewModel()
	{
	}

	public ObservableCollection<string> WorksheetNames { get; } = [];

	public string SelectedWorksheetName
	{
		get { return this.selectedWorksheetName; }
		set { this.SetField(ref this.selectedWorksheetName, value); }
	}

	public override void Confirm(string param)
	{
		GuiManager.Instance.MainViewModel.Dialog = new TableMappingViewModel(this.workbook.Worksheet(this.SelectedWorksheetName));
	}

	private async Task GetExcelSheetNames(string filePath)
	{
		await Task.Run(() => this.workbook = new XLWorkbook(filePath));
		foreach (var sheet in this.workbook.Worksheets)
		{
			this.WorksheetNames.Add(sheet.Name);
		}
	}


	public static Task<ExcelImportDialogViewModel> CreateAsync(string filePath)
	{
		var ret = new ExcelImportDialogViewModel();
		return ret.InitializeAsync(filePath);
	}

	private async Task<ExcelImportDialogViewModel> InitializeAsync(string filePath)
	{
		await Task.Run(() => this.workbook = new XLWorkbook(filePath));
		return this;
	}
}
