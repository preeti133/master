// File: src/Modules/Users/RallyAPI.Users.Endpoints/Customers/SetDefaultAddress.cs

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.Users.Application.Customers.Commands.SetDefaultAddress;
using System.Security.Claims;

namespace RallyAPI.Users.Endpoints.Customers;

public class SetDefaultAddress : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/customers/addresses/{addressId:guid}/default", HandleAsync)
            .WithName("CustomerSetDefaultAddress")
            .WithTags("Customers")
            .RequireAuthorization("Customer");
    }

    private static async Task<IResult> HandleAsync(
        Guid addressId,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var customerId = user.FindFirst("sub")?.Value;
        if (!Guid.TryParse(customerId, out var id))
            return Results.Unauthorized();

        var command = new SetDefaultAddressCommand(id, addressId);
        var result = await sender.Send(command, cancellationToken);

        return result.IsFailure
            ? Results.BadRequest(new { error = result.Error.Message })
            : Results.Ok(new { message = "Default address updated" });
    }
}
