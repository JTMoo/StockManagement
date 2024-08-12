using StockManagement.Gui.Commands;
using System.Windows;
using System.Windows.Controls;

namespace StockManagement.Gui.View.Controls;


public partial class DialogActionBarControl : UserControl
{
	public static readonly DependencyProperty ConfirmCommandProperty = DependencyProperty.Register(
		"ConfirmCommand",
		typeof(RelayCommand<string>),
		typeof(DialogActionBarControl));
	public static readonly DependencyProperty ConfirmCommandVisibilityProperty = DependencyProperty.Register(
		"ConfirmVisibility",
		typeof(Visibility),
		typeof(DialogActionBarControl),
		new PropertyMetadata(Visibility.Visible));
	public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
		"CancelCommand",
		typeof(RelayCommand<string>),
		typeof(DialogActionBarControl));
	public static readonly DependencyProperty CancelCommandVisibilityProperty = DependencyProperty.Register(
		"CancelVisibility",
		typeof(Visibility),
		typeof(DialogActionBarControl),
		new PropertyMetadata(Visibility.Visible));


	public DialogActionBarControl()
    {
        InitializeComponent();
	}

	public RelayCommand<string> ConfirmCommand
	{
		get => (RelayCommand<string>)this.GetValue(ConfirmCommandProperty);
		set => this.SetValue(ConfirmCommandProperty, value);
	}

	public Visibility ConfirmVisibility
	{
		get => (Visibility)this.GetValue(ConfirmCommandVisibilityProperty);
		set => this.SetValue(ConfirmCommandVisibilityProperty, value);
	}

	public RelayCommand<string> CancelCommand
	{
		get => (RelayCommand<string>)this.GetValue(CancelCommandProperty);
		set => this.SetValue(CancelCommandProperty, value);
	}

	public Visibility CancelVisibility
	{
		get => (Visibility)this.GetValue(CancelCommandVisibilityProperty);
		set => this.SetValue(CancelCommandVisibilityProperty, value);
	}
}
