using DocumentFormat.OpenXml.Wordprocessing;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Commands;


public class ChangeSettingsCommand : ICommand
{
	public CommandData Data { get; set; }
	public bool Execute()
	{
		if (Data.Value is not AvailableLanguages value) return false;

		MainManager.Instance.Settings.SelectedLanguage = value;
		DatabaseManager.Update<Settings>(MainManager.Instance.Settings);
		return true;
	}
}