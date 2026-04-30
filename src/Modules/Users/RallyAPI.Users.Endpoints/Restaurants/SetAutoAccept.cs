using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.SharedKernel.Extensions;
using RallyAPI.Users.Application.Restaurants.Commands.SetAutoAccept;
using System.Security.Claims;

namespace RallyAPI.Users.Endpoints.Restaurants;

public class SetAutoAccept : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/restaurants/settings/auto-accept", HandleAsync)
            .WithName("SetAutoAcceptOrders")
            .WithTags("Restaurants")
            .RequireAuthorization("Restaurant");
    }

    public record SetAutoAcceptRequest(bool AutoAcceptOrders);

    private static async Task<IResult> HandleAsync(
        SetAutoAcceptRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var restaurantId = Guid.Parse(user.FindFirstValue("sub")!);

        var command = new SetAutoAcceptCommand(restaurantId, request.AutoAcceptOrders);
        var result = await sender.Send(command, cancellationToken);

        return result.IsFailure
            ? result.Error.ToErrorResult()
            : Results.Ok(new { autoAcceptOrders = request.AutoAcceptOrders });
    }
}
