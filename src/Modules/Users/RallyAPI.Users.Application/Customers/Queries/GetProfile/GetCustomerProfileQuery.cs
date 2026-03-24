using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Customers.Queries.GetProfile;

public sealed record GetCustomerProfileQuery(Guid CustomerId) : IRequest<Result<CustomerProfileResponse>>;

public sealed record CustomerProfileResponse(
    Guid Id,
    string Phone,
    string? Name,
    string? Email,
    bool IsActive,
    List<CustomerAddressResponse> Addresses);

public sealed record CustomerAddressResponse(
    Guid Id,
    string AddressLine,
    string? Landmark,
    decimal Latitude,
    decimal Longitude,
    string Label,
    bool IsDefault,
    string? PlaceId = null);