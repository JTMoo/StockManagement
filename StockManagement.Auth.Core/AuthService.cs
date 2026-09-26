using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Auth.Core;


internal class AuthService(IUserServiceProvider userServiceProvider) : IAuthService
{
	private readonly IUserServiceProvider _userServiceProvider = userServiceProvider;


	public async Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var user = await _userServiceProvider.GetUserByUsernameAsync(username);
		if (user is null || !PasswordHasher.Verify(password, user.PasswordHash)) return null;
		return user;
	}

	public string HashPassword(string password)
	{
		return PasswordHasher.Hash(password);
	}
}
