// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Commands/UpdateAddress/UpdateCustomerAddressCommand.cs

using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Customers.Commands.UpdateAddress;

public sealed record UpdateCustomerAddressCommand(
    Guid CustomerId,
    Guid AddressId,
    string AddressLine,
    string? Landmark,
    decimal Latitude,
    decimal Longitude,
    string Label) : IRequest<Result>;
