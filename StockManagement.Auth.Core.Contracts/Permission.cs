using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Auth.Core.Contracts;


/// <summary>
/// Per-module Read/Write permission strings put on the JWT and checked with FastEndpoints' <c>Permissions()</c>. See ADR-0017.
/// </summary>
public static class Permission
{
	public const string UsersManage = "Users.Manage";
	public const string CustomersRead = "Customers.Read";
	public const string CustomersWrite = "Customers.Write";
	public const string StockItemsRead = "StockItems.Read";
	public const string StockItemsWrite = "StockItems.Write";
	public const string SalesRead = "Sales.Read";
	public const string SalesWrite = "Sales.Write";
	public const string SettingsRead = "Settings.Read";
	public const string SettingsWrite = "Settings.Write";

	public static readonly IReadOnlyList<string> CatalogAll =
	[
		UsersManage,
		CustomersRead, CustomersWrite,
		StockItemsRead, StockItemsWrite,
		SalesRead, SalesWrite,
		SettingsRead, SettingsWrite
	];

	/// <returns>Every permission for <see cref="UserRole.Admin"/>, otherwise <paramref name="user"/>'s stored <see cref="User.Permissions"/></returns>
	public static IReadOnlyList<string> Effective(User user)
	{
		return user.Role == UserRole.Admin ? CatalogAll : user.Permissions;
	}
}
