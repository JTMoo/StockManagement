using System.Collections.ObjectModel;
using ClosedXML.Excel;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class ExcelImportDialogViewModel : DialogViewModelBase
{
	private string selectedWorksheetName = string.Empty;
	private XLWorkbook workbook;


	public ExcelImportDialogViewModel(string filePath)
	{
		GuiManager.Instance.ShowWaitDialog();
		this.GetExcelSheetNames(filePath);
		GuiManager.Instance.HideWaitDialog();
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

	private void GetExcelSheetNames(string filePath)
	{
		this.workbook = new XLWorkbook(filePath);
		foreach (var sheet in this.workbook.Worksheets)
		{
			this.WorksheetNames.Add(sheet.Name);
		}
	}
}
