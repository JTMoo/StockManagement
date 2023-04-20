using StockManagement.Gui.ViewModel;
using StockManagement.Kernel.Model;
using StockManagement.Kernel;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Windows;
using MongoDB.Driver.Linq;
using System.Linq;
using System.IO;
using System.Windows.Markup;
using SharpCompress.Compressors.Xz;
using System.Text;

namespace StockManagement.Gui;


internal class GuiManager
{
	internal static readonly GuiManager Instance = new GuiManager();

	internal MainViewModel MainViewModel { get; private set; }
	internal Dictionary<Type, DialogViewModelBase> StockItemToViewModel { get; private set; } = new Dictionary<Type, DialogViewModelBase>();
	internal Dictionary<Type, DataTemplate> StockItemToView { get; private set; } = new Dictionary<Type, DataTemplate>();


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
		var views = ReflectionManager.GetTypesInNamespace(guiAssembly, "StockManagement.Gui.View.StockItemCreation");
		

		if (this.MainViewModel.StockItemTypes.Count != viewModels.Count)
			throw new ArgumentOutOfRangeException("The amount of StockItemTypes and Creation ViewModels do not match.", innerException: null);

		for (int i = 0; i < this.MainViewModel.StockItemTypes.Count; i++)
		{
			var vm = Activator.CreateInstance(viewModels[i]) as DialogViewModelBase;
			if (vm == null)
				throw new ArgumentNullException("Failed to create Instance of Creation ViewModel.", innerException: null);

			this.StockItemToViewModel[this.MainViewModel.StockItemTypes[i]] = vm;

			if (views[i] == null || views[i].FullName == null)
				throw new ArgumentNullException("Correct Dialog-View was not found.", innerException: null);

			var assemblyName = guiAssembly.GetName().Name;
			var uriToXaml = new Uri("/" + assemblyName + ";component" + views[i].FullName.Replace(assemblyName, string.Empty).Replace(".", "/") + ".xaml", UriKind.Relative);
			
			var stream = Application.GetResourceStream(uriToXaml).Stream;
			var template = (DataTemplate)XamlReader.Load(stream);
			this.StockItemToView[viewModels[i]] = template;

			stream.Close();
		}
	}

	private List<Type> GetStockItemTypes()
	{
		var kernelAssembly = Assembly.Load(new AssemblyName("Stockmanagement.Kernel"));
		return ReflectionManager.GetTypesOfBase(kernelAssembly, typeof(StockItem));
	}
}
