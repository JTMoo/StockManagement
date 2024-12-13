using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.View.Controls;


public partial class LargeTextBoxWithPreviewControl : UserControl
{
	public static readonly DependencyProperty ActualTextProperty = DependencyProperty.Register(
		"ActualText",
		typeof(string),
		typeof(LargeTextBoxWithPreviewControl));
	public static readonly DependencyProperty TextPreviewProperty = DependencyProperty.Register(
		"TextPreview",
		typeof(string),
		typeof(LargeTextBoxWithPreviewControl));

	public LargeTextBoxWithPreviewControl()
	{
		InitializeComponent();
	}

	public string ActualText
	{
		get => (string)GetValue(ActualTextProperty);
		set => SetValue(ActualTextProperty, value);
	}

	public string TextPreview
	{
		get => (string)GetValue(TextPreviewProperty);
		set => SetValue(TextPreviewProperty, value);
	}
}