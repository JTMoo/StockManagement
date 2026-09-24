using System;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Gui.ViewModel;
using StockManagement.Gui.ViewModel.Primary;

namespace StockManagement.Gui;


internal static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the main window's view model and the primary views it switches between
	/// </summary>
	public static IServiceCollection AddGui(this IServiceCollection services)
	{
		services.AddSingleton<MainViewModel>();
		services.AddViewModel<StockItemsViewModel>();
		services.AddViewModel<CustomerViewModel>();
		services.AddViewModel<InvoiceViewModel>();
		services.AddViewModel<LoginViewModel>();
		return services;
	}

	/// <summary>
	/// Registers <typeparamref name="T"/> as transient plus a <see cref="Func{T}"/> that creates a fresh instance on every call
	/// </summary>
	private static IServiceCollection AddViewModel<T>(this IServiceCollection services) where T : class
	{
		services.AddTransient<T>();
		services.AddSingleton<Func<T>>(provider => provider.GetRequiredService<T>);
		return services;
	}
}
