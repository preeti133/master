// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Queries/GetAddresses/GetCustomerAddressesQueryHandler.cs

using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;
using RallyAPI.Users.Application.Customers.Queries.GetProfile;

namespace RallyAPI.Users.Application.Customers.Queries.GetAddresses;

internal sealed class GetCustomerAddressesQueryHandler
    : IRequestHandler<GetCustomerAddressesQuery, Result<List<CustomerAddressResponse>>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerAddressesQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<List<CustomerAddressResponse>>> Handle(
        GetCustomerAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            return Result.Failure<List<CustomerAddressResponse>>(
                Error.NotFound("Customer.NotFound", request.CustomerId));



        var addresses = customer.Addresses
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new CustomerAddressResponse(
                a.Id,
                a.Address.AddressLine,
                a.Address.Landmark,
                a.Address.Latitude,
                a.Address.Longitude,
                a.Address.Label,
                a.IsDefault))
            .ToList();

        return Result<List<CustomerAddressResponse>>.Success(addresses);
    }
}
