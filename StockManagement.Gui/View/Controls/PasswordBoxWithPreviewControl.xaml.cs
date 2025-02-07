using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.View.Controls;


public partial class PasswordBoxWithPreviewControl : UserControl
{
	public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
		"Password",
		typeof(string),
		typeof(PasswordBoxWithPreviewControl));
	public static readonly DependencyProperty PasswordPreviewProperty = DependencyProperty.Register(
		"PasswordPreview",
		typeof(string),
		typeof(PasswordBoxWithPreviewControl));

	public PasswordBoxWithPreviewControl()
	{
		InitializeComponent();
	}

	public string Password
	{
		get => (string)GetValue(PasswordProperty);
		set => SetValue(PasswordProperty, value);
	}

	public string PasswordPreview
	{
		get => (string)GetValue(PasswordPreviewProperty);
		set => SetValue(PasswordPreviewProperty, value);
	}
}
