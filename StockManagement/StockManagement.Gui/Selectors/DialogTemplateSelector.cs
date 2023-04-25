using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.Selectors;


public class DialogTemplateSelector : DataTemplateSelector
{
	private readonly Dictionary<Type, DataTemplate> StockItemToView = new Dictionary<Type, DataTemplate>();

	public DialogTemplateSelector() 
	{
		this.GetStockItemCreationTemplates();
	}

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (item == null || !StockItemToView.ContainsKey(item.GetType()))
			return base.SelectTemplate(item, container);

		return StockItemToView[item.GetType()];
	}

	private void GetStockItemCreationTemplates()
	{
		var resourceDictionary = new ResourceDictionary
		{
			Source = new Uri("/StockManagement.Gui;component/View/StockItemCreation/StockItemCreationDictionary.xaml", UriKind.Relative)
		};

		foreach (var key in resourceDictionary.Keys)
		{
			if (resourceDictionary[key] is DataTemplate dataTemplate && dataTemplate.DataType is Type dataType)
			{
				this.StockItemToView.Add(dataType, dataTemplate);
			}
		}
	}
}