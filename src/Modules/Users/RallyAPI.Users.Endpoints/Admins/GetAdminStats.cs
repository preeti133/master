using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.SharedKernel.Extensions;
using RallyAPI.Users.Application.Admins.Queries.GetStats;
using System.Security.Claims;

namespace RallyAPI.Users.Endpoints.Admins;

public class GetAdminStats : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admins/stats", HandleAsync)
            .WithName("GetAdminStats")
            .WithTags("Admins")
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new GetAdminStatsQuery(adminId), cancellationToken);

        return result.IsFailure
            ? result.Error.ToErrorResult()
            : Results.Ok(result.Value);
    }
}
