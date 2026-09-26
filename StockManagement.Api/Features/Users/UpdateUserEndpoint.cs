using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Users;


/// <param name="Password">Left <see langword="null"/> or empty to keep the current password</param>
public sealed record UpdateUserRequest(string Id, string Username, string FullName = "", string Email = "", string Phone = "", string Position = "", UserRole Role = UserRole.Standard, IReadOnlyList<string>? Permissions = null, string? Password = null);


public class UpdateUserValidator : Validator<UpdateUserRequest>
{
	public UpdateUserValidator()
	{
		this.RuleFor(request => request.Username).NotEmpty();
		this.RuleFor(request => request.Password).MinimumLength(8).When(request => !string.IsNullOrEmpty(request.Password));
		this.RuleFor(request => request.Role).IsInEnum();
		this.RuleForEach(request => request.Permissions).Must(permission => Permission.CatalogAll.Contains(permission));
	}
}


/// <remarks>Matches the stored user by <c>Id</c> (docs/decisions.md: update by Id, never by business key), so Username can be renamed.</remarks>
public class UpdateUserEndpoint(IUserServiceProvider userServiceProvider, IAuthService authService) : Endpoint<UpdateUserRequest, Results<Ok<UserResponse>, NotFound, Conflict<DuplicateUsernameResponse>>>
{
	private readonly IUserServiceProvider _userServiceProvider = userServiceProvider;
	private readonly IAuthService _authService = authService;


	public override void Configure()
	{
		this.Put("/users/{Id}");
		this.Permissions(Permission.UsersManage);
	}

	public override async Task<Results<Ok<UserResponse>, NotFound, Conflict<DuplicateUsernameResponse>>> ExecuteAsync(UpdateUserRequest request, CancellationToken cancellationToken)
	{
		if (await _userServiceProvider.GetUserAsync(request.Id) is not User user) return TypedResults.NotFound();

		user.Username = request.Username;
		user.FullName = request.FullName;
		user.Email = request.Email;
		user.Phone = request.Phone;
		user.Position = request.Position;
		user.Role = request.Role;
		user.Permissions = [.. request.Permissions ?? []];
		if (!string.IsNullOrEmpty(request.Password)) user.PasswordHash = _authService.HashPassword(request.Password);

		try
		{
			await _userServiceProvider.UpdateUserAsync(user);
		}
		catch (UsernameAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateUsernameResponse(request.Username));
		}

		return TypedResults.Ok(UserResponse.From(user));
	}
}
