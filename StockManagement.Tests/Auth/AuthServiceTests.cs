using Moq;
using StockManagement.Auth.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Tests.Auth;


[TestClass]
public sealed class AuthServiceTests
{
	private readonly Mock<IUserServiceProvider> _users = new();


	[TestMethod]
	public async Task ValidateCredentialsAsync_CorrectPassword_ReturnsUser()
	{
		// Arrange
		var user = new User { Username = "ana", PasswordHash = PasswordHasher.Hash("s3cret!") };
		_users.Setup(provider => provider.GetUserByUsernameAsync("ana")).ReturnsAsync(user);

		// Act
		var result = await this.CreateService().ValidateCredentialsAsync("ana", "s3cret!");

		// Assert
		Assert.AreSame(user, result);
	}

	[TestMethod]
	public async Task ValidateCredentialsAsync_WrongPassword_ReturnsNull()
	{
		// Arrange
		var user = new User { Username = "ana", PasswordHash = PasswordHasher.Hash("s3cret!") };
		_users.Setup(provider => provider.GetUserByUsernameAsync("ana")).ReturnsAsync(user);

		// Act
		var result = await this.CreateService().ValidateCredentialsAsync("ana", "wrong");

		// Assert
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task ValidateCredentialsAsync_UnknownUsername_ReturnsNull()
	{
		// Arrange
		_users.Setup(provider => provider.GetUserByUsernameAsync("ghost")).ReturnsAsync((User?)null);

		// Act
		var result = await this.CreateService().ValidateCredentialsAsync("ghost", "whatever");

		// Assert
		Assert.IsNull(result);
	}

	[TestMethod]
	public void HashPassword_ThenVerify_MatchesOriginalPassword()
	{
		// Arrange
		var hash = this.CreateService().HashPassword("s3cret!");

		// Act
		var matches = PasswordHasher.Verify("s3cret!", hash);

		// Assert
		Assert.IsTrue(matches);
	}

	private AuthService CreateService()
	{
		return new AuthService(_users.Object);
	}
}
