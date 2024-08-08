using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Commands;


public class ChangeSettingsCommand : ICommand
{
	public CommandData Data { get; set; }
	public bool Execute()
	{
		if (!(Data.Value is Language value)) return false;

		MainManager.Instance.Settings.SelectedLanguage = value;
		MainManager.Instance.DatabaseManager.SettingsDB.Update();
		return true;
	}
}