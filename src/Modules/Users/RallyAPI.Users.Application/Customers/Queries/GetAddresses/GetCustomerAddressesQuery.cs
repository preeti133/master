// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Queries/GetAddresses/GetCustomerAddressesQuery.cs

using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Customers.Queries.GetProfile;

namespace RallyAPI.Users.Application.Customers.Queries.GetAddresses;

public sealed record GetCustomerAddressesQuery(Guid CustomerId)
    : IRequest<Result<List<CustomerAddressResponse>>>;
