using StockManagement.Gui.ViewModel;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui;


internal class GuiManager
{
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

			SetLanguage(MainManagerFacade.SelectedLanguage.GetEnumDescription());
		}
		catch(Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	internal void ShowWaitDialog()
	{
		Application.Current.Dispatcher.Invoke(() =>
		{
			this.MainViewModel.IsWaitDialogVisible = true;
		});
	}

	internal void HideWaitDialog()
	{
		Application.Current.Dispatcher.Invoke(() =>
		{
			this.MainViewModel.IsWaitDialogVisible = false;
		});
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

	private static bool SetLanguage(string inFiveCharLang)
	{
		if (CultureInfo.CurrentCulture.Name.Equals(inFiveCharLang)) return false;

		var ci = new CultureInfo(inFiveCharLang);
		Thread.CurrentThread.CurrentCulture = ci;
		Thread.CurrentThread.CurrentUICulture = ci;
		Language.Resources.Culture = ci;
		return true;
	}
}