using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.Selectors;


public class DialogTemplateSelector : DataTemplateSelector
{
	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		return base.SelectTemplate(item, container);
	}
}
