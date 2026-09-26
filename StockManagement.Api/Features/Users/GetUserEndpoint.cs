using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Users;


public sealed record GetUserRequest(string Id);


public class GetUserEndpoint(IUserServiceProvider userServiceProvider) : Endpoint<GetUserRequest, Results<Ok<UserResponse>, NotFound>>
{
	private readonly IUserServiceProvider _userServiceProvider = userServiceProvider;


	public override void Configure()
	{
		this.Get("/users/{Id}");
		this.Permissions(Permission.UsersManage);
	}

	public override async Task<Results<Ok<UserResponse>, NotFound>> ExecuteAsync(GetUserRequest request, CancellationToken cancellationToken)
	{
		if (await _userServiceProvider.GetUserAsync(request.Id) is not User user) return TypedResults.NotFound();

		return TypedResults.Ok(UserResponse.From(user));
	}
}
