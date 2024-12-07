using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.View.Controls;


public partial class TextBoxWithPreviewControl : UserControl
{
	public static readonly DependencyProperty ActualTextProperty = DependencyProperty.Register(
		"ActualText",
		typeof(string),
		typeof(TextBoxWithPreviewControl));
	public static readonly DependencyProperty TextPreviewProperty = DependencyProperty.Register(
		"TextPreview",
		typeof(string),
		typeof(TextBoxWithPreviewControl));

	public TextBoxWithPreviewControl()
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