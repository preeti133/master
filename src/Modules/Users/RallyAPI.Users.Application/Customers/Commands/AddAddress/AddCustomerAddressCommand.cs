using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Customers.Commands.AddAddress;

public sealed record AddCustomerAddressCommand(
    Guid CustomerId,
    string AddressLine,
    string? Landmark,
    decimal Latitude,
    decimal Longitude,
    string Label,
    string? PlaceId = null) : IRequest<Result<Guid>>;