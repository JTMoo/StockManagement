using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Users;


public class ListUsersEndpoint(IUserServiceProvider userServiceProvider) : EndpointWithoutRequest<IReadOnlyList<UserResponse>>
{
	private readonly IUserServiceProvider _userServiceProvider = userServiceProvider;


	public override void Configure()
	{
		this.Get("/users");
		this.Permissions(Permission.UsersManage);
	}

	public override async Task<IReadOnlyList<UserResponse>> ExecuteAsync(CancellationToken cancellationToken)
	{
		var users = await _userServiceProvider.GetAllUsersAsync() ?? [];
		return users.Select(UserResponse.From).ToList();
	}
}
