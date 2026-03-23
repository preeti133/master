using Microsoft.EntityFrameworkCore;
using RallyAPI.Orders.Domain.Abstractions;
using RallyAPI.Orders.Domain.ValueObjects;

namespace RallyAPI.Orders.Infrastructure.Services;

/// <summary>
/// Generates unique human-readable order numbers using a Postgres sequence.
/// Format: ORD-{YYYYMMDD}-{SEQUENCE:D5}
/// </summary>
public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly OrdersDbContext _context;

    public OrderNumberGenerator(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<OrderNumber> GenerateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            // Use Postgres sequence for atomic, concurrency-safe generation
            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT nextval('orders.order_number_seq')";

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            var sequence = Convert.ToInt32(result);

            return OrderNumber.Create(sequence, today);
        }
        catch
        {
            // Fallback to timestamp-based generation
            return OrderNumber.CreateFallback();
        }
    }
}