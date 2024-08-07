using StockManagement.Gui.View;
using StockManagement.Gui.ViewModel;
using StockManagement.Kernel;
using System.Globalization;
using System.IO;
using System.Threading;
using System;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Diagnostics;

namespace StockManagement.Gui;


public partial class App
{
    private MainViewModel _mainViewModel;

	private static string? _directory;
	public readonly string FolderName = "Resources";

	public App()
	{
		_directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}


	protected override void OnStartup(StartupEventArgs e)
    {
        MainManager.Instance.Init();

        _mainViewModel = new MainViewModel();
        GuiManager.Instance.Init(_mainViewModel);

		new MainWindow()
        {
			DataContext = _mainViewModel
		}.Show();

		SetLanguageResourceDictionary(GetLocXamlFilePath(CultureInfo.CurrentCulture.Name));

		base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        MainManager.Instance.Dispose();
        base.OnExit(e);
    }

	public void SwitchLanguage(string inFiveCharLang)
	{
		if (CultureInfo.CurrentCulture.Name.Equals(inFiveCharLang))
			return;

		var ci = new CultureInfo(inFiveCharLang);
		Thread.CurrentThread.CurrentCulture = ci;
		Thread.CurrentThread.CurrentUICulture = ci;

		SetLanguageResourceDictionary(GetLocXamlFilePath(inFiveCharLang));
	}

	private string GetLocXamlFilePath(string inFiveCharLang)
	{
		string locXamlFile = "LocalizationDictionary." + inFiveCharLang + ".xaml";
		if (_directory != null) return Path.Combine(_directory, FolderName, locXamlFile);
		
		return string.Empty;
	}

	private void SetLanguageResourceDictionary(string inFile)
	{
		if (!File.Exists(inFile))
		{
			Trace.WriteLine("Couldn't find Language File.");
			return;
		}

		var languageDictionary = new ResourceDictionary
		{
			Source = new Uri(inFile)
		};

		var name = "ResourceDictionaryName";
		var dict = Current.Resources.MergedDictionaries.FirstOrDefault(dict => dict.Contains(name));
		Current.Resources.MergedDictionaries.Remove(dict);

		Resources.MergedDictionaries.Add(languageDictionary);
	}
}