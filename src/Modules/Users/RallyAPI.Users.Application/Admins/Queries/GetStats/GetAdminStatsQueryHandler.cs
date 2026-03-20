using MediatR;
using RallyAPI.SharedKernel.Abstractions.Orders;
using RallyAPI.SharedKernel.Results;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Application.Admins.Queries.GetStats;

internal sealed class GetAdminStatsQueryHandler
    : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRiderRepository _riderRepository;
    private readonly IOrderStatsService _orderStats;

    public GetAdminStatsQueryHandler(
        IAdminRepository adminRepository,
        ICustomerRepository customerRepository,
        IRestaurantRepository restaurantRepository,
        IRiderRepository riderRepository,
        IOrderStatsService orderStats)
    {
        _adminRepository = adminRepository;
        _customerRepository = customerRepository;
        _restaurantRepository = restaurantRepository;
        _riderRepository = riderRepository;
        _orderStats = orderStats;
    }

    public async Task<Result<AdminStatsResponse>> Handle(
        GetAdminStatsQuery request,
        CancellationToken cancellationToken)
    {
        var admin = await _adminRepository.GetByIdAsync(request.RequestedByAdminId, cancellationToken);
        if (admin is null)
            return Result.Failure<AdminStatsResponse>(Error.NotFound("Admin", request.RequestedByAdminId));

        var (totalCustomers, totalRestaurants, totalRiders, onlineRiders,
             totalOrders, activeOrders, todayOrders) = await (
            _customerRepository.CountAsync(cancellationToken),
            _restaurantRepository.CountAsync(cancellationToken),
            _riderRepository.CountAsync(cancellationToken: cancellationToken),
            _riderRepository.CountAsync(isOnline: true, cancellationToken: cancellationToken),
            _orderStats.GetTotalCountAsync(cancellationToken),
            _orderStats.GetActiveCountAsync(cancellationToken),
            _orderStats.GetTodayCountAsync(cancellationToken)
        ).WhenAll();

        return Result.Success(new AdminStatsResponse(
            totalCustomers,
            totalRestaurants,
            totalRiders,
            onlineRiders,
            totalOrders,
            activeOrders,
            todayOrders));
    }
}

file static class TaskExtensions
{
    internal static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(
        this (Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4,
              Task<T5> t5, Task<T6> t6, Task<T7> t7) tasks)
    {
        await Task.WhenAll(tasks.t1, tasks.t2, tasks.t3, tasks.t4,
                           tasks.t5, tasks.t6, tasks.t7);
        return (tasks.t1.Result, tasks.t2.Result, tasks.t3.Result, tasks.t4.Result,
                tasks.t5.Result, tasks.t6.Result, tasks.t7.Result);
    }
}
