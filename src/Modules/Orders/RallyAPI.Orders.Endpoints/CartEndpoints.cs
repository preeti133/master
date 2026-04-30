using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RallyAPI.Orders.Application.Abstractions;
using RallyAPI.SharedKernel.Abstractions;
using RallyAPI.Orders.Application.Cart.Commands.AddCartItem;
using RallyAPI.Orders.Application.Cart.Commands.ClearCart;
using RallyAPI.Orders.Application.Cart.Commands.RemoveCartItem;
using RallyAPI.Orders.Application.Cart.Commands.SyncCart;
using RallyAPI.Orders.Application.Cart.Commands.UpdateCartItem;
using RallyAPI.Orders.Application.Cart.DTOs;
using RallyAPI.Orders.Application.Cart.Queries.GetCart;
using RallyAPI.SharedKernel.Extensions;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Endpoints;

public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart")
            .WithOpenApi()
            .RequireAuthorization("Customer");

        // GET /api/cart
        group.MapGet("/", GetCart)
            .WithName("GetCart")
            .WithSummary("Get the current customer's cart")
            .Produces<CartDto>()
            .Produces(StatusCodes.Status204NoContent);

        // POST /api/cart/items?replaceCart=false
        group.MapPost("/items", AddCartItem)
            .WithName("AddCartItem")
            .WithSummary("Add an item to the cart")
            .Produces<CartDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // PUT /api/cart/items/{itemId}
        group.MapPut("/items/{itemId:guid}", UpdateCartItem)
            .WithName("UpdateCartItem")
            .WithSummary("Update quantity of a cart item")
            .Produces<CartDto>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // DELETE /api/cart/items/{itemId}
        group.MapDelete("/items/{itemId:guid}", RemoveCartItem)
            .WithName("RemoveCartItem")
            .WithSummary("Remove an item from the cart")
            .Produces<CartDto>()
            .Produces(StatusCodes.Status204NoContent);

        // DELETE /api/cart
        group.MapDelete("/", ClearCart)
            .WithName("ClearCart")
            .WithSummary("Clear the entire cart")
            .Produces(StatusCodes.Status204NoContent);

        // POST /api/cart/sync?replaceCart=false
        group.MapPost("/sync", SyncCart)
            .WithName("SyncCart")
            .WithSummary("Merge a guest cart into the server-side cart after login")
            .Produces<CartDto>()
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return app;
    }

    #region Request DTOs

    private sealed record AddCartItemRequest(
        Guid RestaurantId,
        string RestaurantName,
        Guid MenuItemId,
        string Name,
        decimal UnitPrice,
        int Quantity,
        string? Options,
        string? SpecialInstructions);

    private sealed record UpdateCartItemRequest(int Quantity);

    private sealed record SyncCartRequest(
        Guid RestaurantId,
        string RestaurantName,
        List<SyncCartItemDto> Items);

    #endregion

    #region Handlers

    private static async Task<IResult> GetCart(
        IMediator mediator,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            return Results.Unauthorized();

        var result = await mediator.Send(new GetCartQuery(currentUser.UserId.Value), cancellationToken);
        if (!result.IsSuccess)
            return result.Error.ToErrorResult();

        return result.Value == null
            ? Results.NoContent()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> AddCartItem(
        [FromBody] AddCartItemRequest request,
        [FromQuery] bool replaceCart,
        IMediator mediator,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            return Results.Unauthorized();

        var command = new AddCartItemCommand
        {
            CustomerId = currentUser.UserId.Value,
            RestaurantId = request.RestaurantId,
            RestaurantName = request.RestaurantName,
            MenuItemId = request.MenuItemId,
            Name = request.Name,
            UnitPrice = request.UnitPrice,
            Quantity = request.Quantity,
            Options = request.Options,
            SpecialInstructions = request.SpecialInstructions,
            ReplaceCart = replaceCart
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.Error.ToErrorResult();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateCartItem(
        Guid itemId,
        [FromBody] UpdateCartItemRequest request,
        IMediator mediator,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            return Results.Unauthorized();

        var command = new UpdateCartItemCommand
        {
            CustomerId = currentUser.UserId.Value,
            ItemId = itemId,
            Quantity = request.Quantity
        };

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.ToErrorResult();
    }

    private static async Task<IResult> RemoveCartItem(
        Guid itemId,
        IMediator mediator,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            return Results.Unauthorized();

        var command = new RemoveCartItemCommand
        {
            CustomerId = currentUser.UserId.Value,
            ItemId = itemId
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.Error.ToErrorResult();

        return result.Value == null
            ? Results.NoContent()   // cart is now empty
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> ClearCart(
        IMediator mediator,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            return Results.Unauthorized();

        await mediator.Send(new ClearCartCommand(currentUser.UserId.Value), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> SyncCart(
        [FromBody] SyncCartRequest request,
        [FromQuery] bool replaceCart,
        IMediator mediator,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            return Results.Unauthorized();

        var command = new SyncCartCommand
        {
            CustomerId = currentUser.UserId.Value,
            RestaurantId = request.RestaurantId,
            RestaurantName = request.RestaurantName,
            Items = request.Items,
            ReplaceCart = replaceCart
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.Error.ToErrorResult();

        return Results.Ok(result.Value);
    }

    #endregion
}
