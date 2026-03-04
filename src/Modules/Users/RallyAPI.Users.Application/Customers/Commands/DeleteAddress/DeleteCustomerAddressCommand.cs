// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Commands/DeleteAddress/DeleteCustomerAddressCommand.cs

using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Customers.Commands.DeleteAddress;

public sealed record DeleteCustomerAddressCommand(
    Guid CustomerId,
    Guid AddressId) : IRequest<Result>;
