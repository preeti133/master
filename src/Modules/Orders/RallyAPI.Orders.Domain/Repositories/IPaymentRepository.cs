// File: src/Modules/Orders/RallyAPI.Orders.Domain/Repositories/IPaymentRepository.cs

using RallyAPI.Orders.Domain.Entities;

namespace RallyAPI.Orders.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Payment?> GetByTxnIdAsync(string txnId, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
}