using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.SharedKernel.Extensions;
using RallyAPI.Users.Application.Owners.Commands.Login;

namespace RallyAPI.Users.Endpoints.Owners;

public class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/owners/login", HandleAsync)
            .WithName("OwnerLogin")
            .WithTags("Owners")
            .WithSummary("Authenticate a restaurant owner (super-admin for multiple outlets)")
            .AllowAnonymous()
            .RequireRateLimiting("login");
    }

    public record OwnerLoginRequest(string Email, string Password);

    private static async Task<IResult> HandleAsync(
        OwnerLoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LoginOwnerCommand(request.Email, request.Password);
        var result = await sender.Send(command, cancellationToken);

        return result.IsFailure
            ? result.Error.ToErrorResult()
            : Results.Ok(result.Value);
    }
}
