using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.Orders.Application.Queries.GetEscalatedOrders;
using RallyAPI.SharedKernel.Extensions;

namespace RallyAPI.Users.Endpoints.Admins;

public class GetEscalatedOrders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admins/orders/escalated", HandleAsync)
            .WithName("GetEscalatedOrders")
            .WithTags("Admins")
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> HandleAsync(
        int page,
        int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetEscalatedOrdersQuery(
            Page: page > 0 ? page : 1,
            PageSize: pageSize is > 0 and <= 100 ? pageSize : 20);

        var result = await sender.Send(query, cancellationToken);

        return result.IsFailure
            ? result.Error.ToErrorResult()
            : Results.Ok(result.Value);
    }
}
