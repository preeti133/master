// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Commands/SetDefaultAddress/SetDefaultAddressCommandHandler.cs

using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Customers.Commands.SetDefaultAddress;

internal sealed class SetDefaultAddressCommandHandler
    : IRequestHandler<SetDefaultAddressCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetDefaultAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetDefaultAddressCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            return Result.Failure(Error.NotFound("Customer.NotFound", request.CustomerId));

        var result = customer.SetDefaultAddress(request.AddressId);

        if (result.IsFailure)
            return result;

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
