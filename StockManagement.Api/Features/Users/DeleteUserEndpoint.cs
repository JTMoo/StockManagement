using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Users;


public sealed record DeleteUserRequest(string Id);


public sealed record SelfDeleteResponse;


/// <remarks>Refuses to delete the caller's own account (409), so an admin can't lock themselves out.</remarks>
public class DeleteUserEndpoint(IUserServiceProvider userServiceProvider) : Endpoint<DeleteUserRequest, Results<NoContent, NotFound, Conflict<SelfDeleteResponse>>>
{
	private readonly IUserServiceProvider _userServiceProvider = userServiceProvider;


	public override void Configure()
	{
		this.Delete("/users/{Id}");
		this.Permissions(Permission.UsersManage);
	}

	public override async Task<Results<NoContent, NotFound, Conflict<SelfDeleteResponse>>> ExecuteAsync(DeleteUserRequest request, CancellationToken cancellationToken)
	{
		if (request.Id == this.User.FindFirst("sub")?.Value) return TypedResults.Conflict(new SelfDeleteResponse());
		if (await _userServiceProvider.GetUserAsync(request.Id) is not User user) return TypedResults.NotFound();

		await _userServiceProvider.DeleteUserAsync(user);
		return TypedResults.NoContent();
	}
}
