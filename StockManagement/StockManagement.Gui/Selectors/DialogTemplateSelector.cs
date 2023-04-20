using System;
using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.Selectors;


public class DialogTemplateSelector : DataTemplateSelector
{
	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (item == null || !GuiManager.Instance.StockItemToView.ContainsKey((Type)item))
			return base.SelectTemplate(item, container);

		return GuiManager.Instance.StockItemToView[(Type)item];
	}
}
