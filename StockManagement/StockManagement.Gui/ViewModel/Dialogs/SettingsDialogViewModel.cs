using System.ComponentModel;
using System.Windows;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class SettingsDialogViewModel : DialogViewModelBase
{
	private AvailableLanguages _selectedLanguage;
	private bool languageChangedOnce = false;

	public SettingsDialogViewModel()
	{
		this.SelectedLanguage = MainManagerFacade.SelectedLanguage;
		this.PropertyChanged += this.OnPropertyChangedEvent;
	}

	public AvailableLanguages SelectedLanguage
	{
		get => _selectedLanguage;
		set => this.SetField(ref this._selectedLanguage, value);
	}

	private void OnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(this.SelectedLanguage) || languageChangedOnce) return;

		languageChangedOnce = true;
		MessageBox.Show(Language.Resources.languageChangedMessage, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
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

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}