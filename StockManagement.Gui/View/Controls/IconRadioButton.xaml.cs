using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FontAwesome.WPF;

namespace StockManagement.Gui.View.Controls;


public partial class IconRadioButton : UserControl
{
	public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
		"IsChecked",
		typeof(bool),
		typeof(IconRadioButton));
	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
		"Command",
		typeof(ICommand),
		typeof(IconRadioButton));
	public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
		"Icon",
		typeof(FontAwesomeIcon),
		typeof(IconRadioButton));
	public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(
		"IconVisibility",
		typeof(bool),
		typeof(IconRadioButton),
		new PropertyMetadata(true));
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
		"Text",
		typeof(string),
		typeof(IconRadioButton));
	public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
		"GroupName",
		typeof(string),
		typeof(IconRadioButton));
	public static readonly DependencyProperty TextVisibilityProperty = DependencyProperty.Register(
		"TextVisibility",
		typeof(bool),
		typeof(IconRadioButton),
		new PropertyMetadata(true));


	public IconRadioButton()
	{
		InitializeComponent();
	}


	public bool IsChecked
	{
		get => (bool)GetValue(IsCheckedProperty);
		set => SetValue(IsCheckedProperty, value);
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

	public string GroupName
	{
		get => (string)GetValue(GroupNameProperty);
		set => SetValue(TextProperty, value);
	}

	public bool TextVisibility
	{
		get => (bool)GetValue(TextVisibilityProperty);
		set => SetValue(TextVisibilityProperty, value);
	}
}
