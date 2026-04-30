using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using RallyAPI.Users.Domain.Entities;
using RallyAPI.Users.Domain.ValueObjects;
using RallyAPI.Users.Infrastructure.Persistence;

namespace RallyAPI.Host.DevEndpoints;

/// <summary>
/// Developer-only endpoint: creates a RestaurantOwner (with bcrypt-hashed password)
/// and optionally adopts existing restaurants as outlets by setting their OwnerId.
/// Available in Development environment only.
/// </summary>
public static class SeedRestaurantOwner
{
    public record Request(
        string Name,
        string Email,
        string Password,
        string Phone,
        Guid[]? AdoptRestaurantIds);

    public record Response(
        Guid OwnerId,
        string Email,
        int OutletsAdopted,
        bool Created);

    public static IEndpointRouteBuilder MapSeedRestaurantOwner(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/dev/owners/seed", HandleAsync)
            .WithName("DevSeedRestaurantOwner")
            .WithTags("Dev")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] Request request,
        [FromServices] IHostEnvironment env,
        [FromServices] UsersDbContext db,
        CancellationToken ct)
    {
        if (!env.IsDevelopment())
            return Results.NotFound();

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Phone))
        {
            return Results.BadRequest(new { error = "name, email, password, and phone are required" });
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Results.BadRequest(new { error = emailResult.Error.Message });

        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure)
            return Results.BadRequest(new { error = phoneResult.Error.Message });

        var email = emailResult.Value;

        var existing = await db.RestaurantOwners
            .FirstOrDefaultAsync(o => o.Email == email, ct);

        RestaurantOwner owner;
        bool created;
        if (existing is not null)
        {
            owner = existing;
            created = false;
        }
        else
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);
            var createResult = RestaurantOwner.Create(request.Name, email, passwordHash, phoneResult.Value);
            if (createResult.IsFailure)
                return Results.BadRequest(new { error = createResult.Error.Message });

            owner = createResult.Value;
            db.RestaurantOwners.Add(owner);
            created = true;
        }

        var adoptIds = request.AdoptRestaurantIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        var adopted = 0;

        if (adoptIds.Length > 0)
        {
            var restaurants = await db.Restaurants
                .Where(r => adoptIds.Contains(r.Id))
                .ToListAsync(ct);

            foreach (var restaurant in restaurants)
            {
                var setResult = restaurant.SetOwner(owner.Id);
                if (setResult.IsSuccess) adopted++;
            }
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new Response(
            OwnerId: owner.Id,
            Email: owner.Email.Value,
            OutletsAdopted: adopted,
            Created: created));
    }
}
