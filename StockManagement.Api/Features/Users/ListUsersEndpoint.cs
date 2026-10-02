using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Users;


public sealed record ListUsersRequest(string? Cursor, int? PageSize);


/// <param name="NextCursor">Opaque cursor for the next page; <see langword="null"/> on the last page</param>
public sealed record UserListResponse(IReadOnlyList<UserResponse> Items, string? NextCursor);


public class ListUsersEndpoint(IUserServiceProvider userServiceProvider) : Endpoint<ListUsersRequest, UserListResponse>
{
	private readonly IUserServiceProvider _userServiceProvider = userServiceProvider;


	public override void Configure()
	{
		this.Get("/users");
		this.Permissions(Permission.UsersManage);
	}

	public override async Task<UserListResponse> ExecuteAsync(ListUsersRequest request, CancellationToken cancellationToken)
	{
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _userServiceProvider.GetUsersAsync(request.Cursor, pageSize, cancellationToken);
		return new(result.Items.Select(UserResponse.From).ToList(), result.NextCursor);
	}
}
