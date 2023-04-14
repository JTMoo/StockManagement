using StockManagement.Gui.ViewModel;
using StockManagement.Kernel.Model;
using StockManagement.Kernel;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Windows;
using MongoDB.Driver.Linq;
using System.Linq;

namespace StockManagement.Gui;


internal class GuiManager
{
	internal static readonly GuiManager Instance = new GuiManager();

	internal MainViewModel MainViewModel { get; private set; }
	internal Dictionary<Type, DialogViewModelBase> StockItemToViewModel { get; private set; } = new Dictionary<Type, DialogViewModelBase>();


	public void Init(MainViewModel mainViewModel)
	{
		this.MainViewModel = mainViewModel;
		try
		{
			this.MainViewModel.StockItemTypes = this.GetStockItemTypes();
			this.AssignDialogs();
		}
		catch(Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	private void AssignDialogs()
	{
		var guiAssembly = Assembly.GetExecutingAssembly();
		var viewModels = ReflectionManager.GetTypesInNamespace(guiAssembly, "StockManagement.Gui.ViewModel.StockItemCreation")
			.Where(type => type.Name.Contains("ViewModel")).ToList();

		if (this.MainViewModel.StockItemTypes.Count != viewModels.Count)
			throw new ArgumentOutOfRangeException("The amount of StockItemTypes and Creation ViewModels do not match.", innerException: null);

		for (int i = 0; i < this.MainViewModel.StockItemTypes.Count; i++)
		{
			var vm = Activator.CreateInstance(viewModels[i]) as DialogViewModelBase;
			if (vm == null)
				throw new ArgumentNullException("Failed to create Instance of Creation ViewModel.", innerException: null);

			this.StockItemToViewModel[this.MainViewModel.StockItemTypes[i]] = vm;
		}
	}

	private List<Type> GetStockItemTypes()
	{
		var kernelAssembly = Assembly.Load(new AssemblyName("Stockmanagement.Kernel"));
		return ReflectionManager.GetTypesOfBase(kernelAssembly, typeof(StockItem));
	}
}
