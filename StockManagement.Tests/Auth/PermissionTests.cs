using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Tests.Auth;


[TestClass]
public sealed class PermissionTests
{
	[TestMethod]
	public void Effective_AdminUser_ReturnsEveryPermission()
	{
		// Arrange
		var user = new User { Role = UserRole.Admin, Permissions = [] };

		// Act
		var result = Permission.Effective(user);

		// Assert
		CollectionAssert.AreEquivalent(Permission.CatalogAll.ToList(), result.ToList());
	}

	[TestMethod]
	public void Effective_StandardUser_ReturnsOnlyStoredPermissions()
	{
		// Arrange
		var user = new User { Role = UserRole.Standard, Permissions = [Permission.CustomersRead] };

		// Act
		var result = Permission.Effective(user);

		// Assert
		CollectionAssert.AreEqual(new[] { Permission.CustomersRead }, result.ToList());
	}
}
