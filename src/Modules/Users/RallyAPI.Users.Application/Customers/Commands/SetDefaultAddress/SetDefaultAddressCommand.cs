// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Commands/SetDefaultAddress/SetDefaultAddressCommand.cs

using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Customers.Commands.SetDefaultAddress;

public sealed record SetDefaultAddressCommand(
    Guid CustomerId,
    Guid AddressId) : IRequest<Result>;
