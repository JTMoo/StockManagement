using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Commands;


public class ChangeSettingsCommand : ICommand
{
	public CommandData Data { get; set; }
	public bool Execute()
	{
		if (Data.Value is not AvailableLanguages value) return false;

		MainManager.Instance.Settings.SelectedLanguage = value;
		Database.SettingsDataAccess.Update();
		return true;
	}
}