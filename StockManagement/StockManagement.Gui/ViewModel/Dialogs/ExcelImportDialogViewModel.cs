using System.Collections.ObjectModel;
using ClosedXML.Excel;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class ExcelImportDialogViewModel : DialogViewModelBase
{
	private string selectedWorksheetName = string.Empty;


	public ExcelImportDialogViewModel(string filePath)
	{
		this.GetExcelSheetNames(filePath);
	}

	public ObservableCollection<string> WorksheetNames { get; set; }

	public string SelectedWorksheetName
	{
		get { return this.selectedWorksheetName; }
		set { this.SetField(ref this.selectedWorksheetName, value); }
	}

	private void GetExcelSheetNames(string filePath)
	{
		var workbook = new XLWorkbook(filePath);
		foreach (var sheet in workbook.Worksheets)
		{
			this.WorksheetNames.Add(sheet.Name);
		}
	}
}
