// File: src/Modules/Orders/RallyAPI.Orders.Infrastructure/Persistence/Repositories/PaymentRepository.cs

using Microsoft.EntityFrameworkCore;
using RallyAPI.Orders.Domain.Entities;
using RallyAPI.Orders.Domain.Repositories;

namespace RallyAPI.Orders.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly OrdersDbContext _context;

    public PaymentRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

    public async Task<Payment?> GetByTxnIdAsync(string txnId, CancellationToken ct = default)
        => await _context.Payments.FirstOrDefaultAsync(p => p.TxnId == txnId, ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        await _context.Payments.AddAsync(payment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(ct);
    }
}