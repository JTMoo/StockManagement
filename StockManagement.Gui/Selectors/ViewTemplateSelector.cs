using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.Selectors;


public class ViewTemplateSelector : DataTemplateSelector
{
	private readonly Dictionary<Type, DataTemplate> ViewModelToView = [];

	public ViewTemplateSelector()
	{
		this.GetDialogTemplates();
	}

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (item == null || !ViewModelToView.ContainsKey(item.GetType()))
			return base.SelectTemplate(item, container);

		return ViewModelToView[item.GetType()];
	}

	private void GetDialogTemplates()
	{
		var resourceDictionary = new ResourceDictionary
		{
			Source = new Uri("/StockManagement.Gui;component/View/Dictionaries/PrimaryViewTemplateSelectorDictionary.xaml", UriKind.Relative)
		};

		foreach (var key in resourceDictionary.Keys)
		{
			if (resourceDictionary[key] is DataTemplate dataTemplate && dataTemplate.DataType is Type dataType)
			{
				this.ViewModelToView.Add(dataType, dataTemplate);
			}
		}
	}
}