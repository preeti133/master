using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.SharedKernel.Extensions;
using RallyAPI.Users.Application.Admins.Queries.GetRiderKycDocuments;
using System.Security.Claims;

namespace RallyAPI.Users.Endpoints.Admins;

public class GetRiderKycDocuments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/riders/{riderId:guid}/kyc-documents", HandleAsync)
            .WithName("GetRiderKycDocuments")
            .WithTags("Admins")
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> HandleAsync(
        Guid riderId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(
            new GetRiderKycDocumentsQuery(adminId, riderId), cancellationToken);

        return result.IsFailure
            ? result.Error.ToErrorResult()
            : Results.Ok(result.Value);
    }
}
