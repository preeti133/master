using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;
using RallyAPI.Users.Domain.ValueObjects;

namespace RallyAPI.Users.Application.Customers.Commands.AddAddress;

internal sealed class AddCustomerAddressCommandHandler 
    : IRequestHandler<AddCustomerAddressCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCustomerAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure<Guid>(Error.NotFound("Customer", request.CustomerId));

        var addressResult = Address.Create(
            request.AddressLine,
            request.Landmark,
            request.Latitude,
            request.Longitude,
            request.Label,
            request.PlaceId);

        if (addressResult.IsFailure)
            return Result.Failure<Guid>(addressResult.Error);

        var addResult = customer.AddAddress(addressResult.Value);
        if (addResult.IsFailure)
            return Result.Failure<Guid>(addResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the new address ID
        var newAddress = customer.Addresses.Last();
        return Result.Success(newAddress.Id);
    }
}