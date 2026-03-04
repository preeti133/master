// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Commands/UpdateAddress/UpdateCustomerAddressCommandHandler.cs

using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;
using RallyAPI.Users.Domain.ValueObjects;

namespace RallyAPI.Users.Application.Customers.Commands.UpdateAddress;

internal sealed class UpdateCustomerAddressCommandHandler
    : IRequestHandler<UpdateCustomerAddressCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateCustomerAddressCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            return Result.Failure(Error.NotFound("Customer.NotFound", request.CustomerId));

        var address = customer.Addresses.FirstOrDefault(a => a.Id == request.AddressId);

        if (address is null)
            return Result.Failure(Error.NotFound("Address.NotFound", request.AddressId));

        var newAddress = Address.Create(
            request.AddressLine,
            request.Landmark,
            request.Latitude,
            request.Longitude,
            request.Label);

        if (newAddress.IsFailure)
            return Result.Failure(newAddress.Error);

        address.UpdateAddress(newAddress.Value);

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
