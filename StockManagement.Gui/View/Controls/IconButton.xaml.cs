using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FontAwesome.WPF;

namespace StockManagement.Gui.View.Controls;


public partial class IconButton : UserControl
{
	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
		"Command",
		typeof(ICommand),
		typeof(IconButton));
	public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
		"Icon",
		typeof(FontAwesomeIcon),
		typeof(IconButton));
	public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(
		"IconVisibility",
		typeof(bool),
		typeof(IconButton),
		new PropertyMetadata(true));
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
		"Text",
		typeof(string),
		typeof(IconButton));
	public static readonly DependencyProperty TextVisibilityProperty = DependencyProperty.Register(
		"TextVisibility",
		typeof(bool),
		typeof(IconButton),
		new PropertyMetadata(true));


	public IconButton()
	{
		InitializeComponent();
	}


	public ICommand Command
	{
		get => (ICommand)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public FontAwesomeIcon Icon
	{
		get => (FontAwesomeIcon)GetValue(IconProperty);
		set => SetValue(IconProperty, value);
	}

	public bool IconVisibility
	{
		get => (bool)GetValue(IconVisibilityProperty);
		set => SetValue(IconVisibilityProperty, value);
	}

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public bool TextVisibility
	{
		get => (bool)GetValue(TextVisibilityProperty);
		set => SetValue(TextVisibilityProperty, value);
	}
}
