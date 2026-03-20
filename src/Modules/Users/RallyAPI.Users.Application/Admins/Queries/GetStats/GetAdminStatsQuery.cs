using MediatR;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Users.Application.Admins.Queries.GetStats;

public sealed record GetAdminStatsQuery(Guid RequestedByAdminId)
    : IRequest<Result<AdminStatsResponse>>;

public sealed record AdminStatsResponse(
    int TotalCustomers,
    int TotalRestaurants,
    int TotalRiders,
    int OnlineRiders,
    int TotalOrders,
    int ActiveOrders,
    int TodayOrders);
