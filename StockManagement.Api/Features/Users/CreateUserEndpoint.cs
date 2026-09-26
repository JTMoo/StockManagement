using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Users;


public sealed record CreateUserRequest(string Username, string Password, string FullName = "", string Email = "", string Phone = "", string Position = "", UserRole Role = UserRole.Standard, IReadOnlyList<string>? Permissions = null);


/// <param name="Code">The username that already exists</param>
public sealed record DuplicateUsernameResponse(string Code);


public class CreateUserValidator : Validator<CreateUserRequest>
{
	public CreateUserValidator()
	{
		this.RuleFor(request => request.Username).NotEmpty();
		this.RuleFor(request => request.Password).NotEmpty().MinimumLength(8);
		this.RuleFor(request => request.Role).IsInEnum();
		this.RuleForEach(request => request.Permissions).Must(permission => Permission.CatalogAll.Contains(permission));
	}
}


public class CreateUserEndpoint(IUserServiceProvider userServiceProvider, IAuthService authService) : Endpoint<CreateUserRequest, Results<Created<UserResponse>, Conflict<DuplicateUsernameResponse>>>
{
	private readonly IUserServiceProvider _userServiceProvider = userServiceProvider;
	private readonly IAuthService _authService = authService;


	public override void Configure()
	{
		this.Post("/users");
		this.Permissions(Permission.UsersManage);
	}

	public override async Task<Results<Created<UserResponse>, Conflict<DuplicateUsernameResponse>>> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken)
	{
		var user = new User
		{
			Username = request.Username,
			PasswordHash = _authService.HashPassword(request.Password),
			FullName = request.FullName,
			Email = request.Email,
			Phone = request.Phone,
			Position = request.Position,
			Role = request.Role,
			Permissions = [.. request.Permissions ?? []]
		};

		try
		{
			await _userServiceProvider.AddUserAsync(user);
		}
		catch (UsernameAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateUsernameResponse(request.Username));
		}

		return TypedResults.Created($"/api/users/{user.Id}", UserResponse.From(user));
	}
}
