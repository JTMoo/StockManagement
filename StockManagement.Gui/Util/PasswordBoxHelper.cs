using System.Windows.Controls;
using System.Windows;

namespace StockManagement.Gui.Util;


public static class PasswordBoxHelper
{
	public static readonly DependencyProperty BoundPassword = DependencyProperty.RegisterAttached(
		"BoundPassword",
		typeof(string),
		typeof(PasswordBoxHelper),
		new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

	public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached(
		"BindPassword",
		typeof(bool),
		typeof(PasswordBoxHelper),
		new PropertyMetadata(false, OnBindPasswordChanged));

	private static readonly DependencyProperty UpdatingPassword = DependencyProperty.RegisterAttached(
		"UpdatingPassword",
		typeof(bool),
		typeof(PasswordBoxHelper),
		new PropertyMetadata(false));

	private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!GetBindPassword(d) || d is not PasswordBox box) return;

		// avoid recursive updating by ignoring the box's changed event
		box.PasswordChanged -= HandlePasswordChanged;

		var newPassword = (string)e.NewValue;
		if (!GetUpdatingPassword(box))
		{
			box.Password = newPassword;
		}

		box.PasswordChanged += HandlePasswordChanged;
	}

	private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
	{
		if (dp == null || dp is not PasswordBox box) return;

		bool wasBound = (bool)(e.OldValue);
		bool needToBind = (bool)(e.NewValue);

		if (wasBound)
		{
			box.PasswordChanged -= HandlePasswordChanged;
		}

		if (needToBind)
		{
			box.PasswordChanged += HandlePasswordChanged;
		}
	}

	private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
	{
		if (sender is not PasswordBox box) return;

		// set a flag to indicate that we're updating the password
		SetUpdatingPassword(box, true);
		// push the new password into the BoundPassword property
		SetBoundPassword(box, box.Password);
		SetUpdatingPassword(box, false);
	}

	public static void SetBindPassword(DependencyObject dp, bool value)
	{
		dp.SetValue(BindPassword, value);
	}

	public static bool GetBindPassword(DependencyObject dp)
	{
		return (bool)dp.GetValue(BindPassword);
	}

	public static string GetBoundPassword(DependencyObject dp)
	{
		return (string)dp.GetValue(BoundPassword);
	}

	public static void SetBoundPassword(DependencyObject dp, string value)
	{
		dp.SetValue(BoundPassword, value);
	}

	private static bool GetUpdatingPassword(DependencyObject dp)
	{
		return (bool)dp.GetValue(UpdatingPassword);
	}

	private static void SetUpdatingPassword(DependencyObject dp, bool value)
	{
		dp.SetValue(UpdatingPassword, value);
	}
}