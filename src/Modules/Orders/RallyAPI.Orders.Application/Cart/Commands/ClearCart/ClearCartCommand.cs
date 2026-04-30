using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Cart.Commands.ClearCart;

public record ClearCartCommand(Guid CustomerId) : IRequest<Result>;
