using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Customers.Queries.GetProfile;

internal sealed class GetCustomerProfileQueryHandler 
    : IRequestHandler<GetCustomerProfileQuery, Result<CustomerProfileResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerProfileQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerProfileResponse>> Handle(
        GetCustomerProfileQuery request, 
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure<CustomerProfileResponse>(Error.NotFound("Customer", request.CustomerId));

        var addresses = customer.Addresses.Select(a => new CustomerAddressResponse(
            a.Id,
            a.Address.AddressLine,
            a.Address.Landmark,
            a.Address.Latitude,
            a.Address.Longitude,
            a.Address.Label,
            a.IsDefault,
            a.Address.PlaceId)).ToList();

        var response = new CustomerProfileResponse(
            customer.Id,
            customer.Phone.GetFormatted(),
            customer.Name,
            customer.Email?.Value,
            customer.IsActive,
            addresses);

        return Result.Success(response);
    }
}