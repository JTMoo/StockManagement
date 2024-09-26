using StockManagement.Gui.ViewModel;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui;


internal class GuiManager
{
	private static string? _directory;
	public readonly string FolderName = "Resources";


	internal static readonly GuiManager Instance = new();
	internal MainViewModel MainViewModel { get; private set; }
	internal Dictionary<Type, DialogViewModelBase> StockItemToViewModel { get; } = [];


	public void Init(MainViewModel mainViewModel)
	{
		this.MainViewModel = mainViewModel;
		try
		{
			this.MainViewModel.StockItemTypes = GetStockItemTypes();
			this.AssignDialogs();
			_directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			SetLanguageResourceDictionary(GetLocXamlFilePath(MainManager.Instance.Settings.SelectedLanguage.GetEnumDescription()));
		}
		catch(Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	public void ChangeLanguage(string inFiveCharLang)
	{
		if (CultureInfo.CurrentCulture.Name.Equals(inFiveCharLang))
			return;

		var ci = new CultureInfo(inFiveCharLang);
		Thread.CurrentThread.CurrentCulture = ci;
		Thread.CurrentThread.CurrentUICulture = ci;

		SetLanguageResourceDictionary(GetLocXamlFilePath(inFiveCharLang));
	}

	private void AssignDialogs()
	{
		var guiAssembly = Assembly.GetExecutingAssembly();
		var stockItemCreationViewModels = ReflectionManager.GetTypesInNamespace(guiAssembly, "StockManagement.Gui.ViewModel.StockItemCreation")
			.Where(type => type.Name.Contains("ViewModel")).ToList();

		if (this.MainViewModel.StockItemTypes.Count != stockItemCreationViewModels.Count)
			throw new ArgumentOutOfRangeException("The amount of StockItemTypes and Creation ViewModels do not match.", innerException: null);

		for (int i = 0; i < this.MainViewModel.StockItemTypes.Count; i++)
		{
			var vm = Activator.CreateInstance(stockItemCreationViewModels[i]) as DialogViewModelBase;
			this.StockItemToViewModel[this.MainViewModel.StockItemTypes[i]] = vm ?? throw new ArgumentNullException("Failed to create Instance of Creation ViewModel.", innerException: null);
		}
	}

	private static List<Type> GetStockItemTypes()
	{
		var kernelAssembly = Assembly.Load(new AssemblyName("Stockmanagement.Kernel"));
		return ReflectionManager.GetTypesOfBase(kernelAssembly, typeof(StockItem));
	}

	private string GetLocXamlFilePath(string inFiveCharLang)
	{
		string locXamlFile = "LocalizationDictionary." + inFiveCharLang + ".xaml";
		if (_directory != null) return Path.Combine(_directory, FolderName, locXamlFile);

		return string.Empty;
	}

	private static void SetLanguageResourceDictionary(string inFile)
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
		var dict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(dict => dict.Contains(name));
		Application.Current.Resources.MergedDictionaries.Remove(dict);
		Application.Current.Resources.MergedDictionaries.Add(languageDictionary);
	}
}