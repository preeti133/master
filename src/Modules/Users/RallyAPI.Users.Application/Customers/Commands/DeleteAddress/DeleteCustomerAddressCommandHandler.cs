// File: src/Modules/Users/RallyAPI.Users.Application/Customers/Commands/DeleteAddress/DeleteCustomerAddressCommandHandler.cs

using MediatR;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Customers.Commands.DeleteAddress;

internal sealed class DeleteCustomerAddressCommandHandler
    : IRequestHandler<DeleteCustomerAddressCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCustomerAddressCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            return Result.Failure(Error.NotFound("Customer.NotFound", request.CustomerId));

        var result = customer.RemoveAddress(request.AddressId);

        if (result.IsFailure)
            return result;

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
