using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model.ExtensionMethods;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class SettingsDialogViewModel : DialogViewModelBase
{
	private Language _selectedLanguage;

	public SettingsDialogViewModel()
	{
		this.SelectedLanguage = MainManager.Instance.Settings.SelectedLanguage;
	}

	public Language SelectedLanguage
	{
		get => _selectedLanguage;
		set => this.SetField(ref this._selectedLanguage, value);
	}


	public override void Confirm(string obj)
	{
		var command = new ChangeSettingsCommand()
		{
			Data = new CommandData()
			{
				Value = this.SelectedLanguage
			}
		};

		GuiManager.Instance.ChangeLanguage(this.SelectedLanguage.GetEnumDescription());
		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}