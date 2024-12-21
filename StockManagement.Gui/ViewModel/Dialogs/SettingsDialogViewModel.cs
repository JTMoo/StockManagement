using System;
using System.ComponentModel;
using System.Windows;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class SettingsDialogViewModel : DialogViewModelBase
{
	private readonly double screenSize = Application.Current.MainWindow.ActualHeight * Application.Current.MainWindow.ActualWidth;
	private readonly double heightWidth = Application.Current.MainWindow.ActualHeight + Application.Current.MainWindow.ActualWidth;

	private AvailableLanguages _selectedLanguage;
	private int _borderAreaInPercent;
	private bool languageChangedOnce = false;

	public SettingsDialogViewModel()
	{
		this.SelectedLanguage = MainManagerFacade.Settings.SelectedLanguage;
		this.BorderAreaInPercent = this.CalculateBorderAreaInPercent();

		this.PropertyChanged += this.OnPropertyChangedEvent;
	}

	#region Properties
	public AvailableLanguages SelectedLanguage
	{
		get => _selectedLanguage;
		set => this.SetField(ref this._selectedLanguage, value);
	}

	public int BorderAreaInPercent
	{
		get => _borderAreaInPercent;
		set => this.SetField(ref this._borderAreaInPercent, value);
	}
	#endregion Properties

	public override void Confirm()
	{
		var command = new ChangeSettingsCommand()
		{
			Data = new CommandData()
			{
				Value = new Settings(this.SelectedLanguage, this.CalculateBorderThickness())
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm();
	}
	private void OnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(this.SelectedLanguage) || languageChangedOnce) return;

		languageChangedOnce = true;
		MessageBox.Show(Language.Resources.languageChangedMessage, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
	}

	private int CalculateBorderThickness()
	{
		var a = -400 / screenSize;
		var b = 200 * heightWidth / screenSize;
		var c = -1 * this.BorderAreaInPercent;
		var borderThickness = (-b + Math.Sqrt(Math.Pow(b, 2) - 4 * a * c)) / (2 * a);
		return Convert.ToInt32(Math.Round(borderThickness, 0));
	}
	
	private int CalculateBorderAreaInPercent()
	{
		var a = -400 / screenSize;
		var b = 200 * heightWidth / screenSize;
		return Convert.ToInt32(a * Math.Pow(MainManagerFacade.Settings.DialogBorderThickness, 2) + b * MainManagerFacade.Settings.DialogBorderThickness);
	}
}