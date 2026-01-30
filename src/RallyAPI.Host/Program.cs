using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RallyAPI.Catalog.Endpoints;
using RallyAPI.Catalog.Infrastructure;
using RallyAPI.Integrations.ProRouting;
using RallyAPI.Orders.Endpoints;
using RallyAPI.Orders.Infrastructure;
using RallyAPI.SharedKernel.Abstractions.Delivery;
using RallyAPI.SharedKernel.Extensions;
using RallyAPI.Users.Endpoints;
using RallyAPI.Users.Infrastructure;
using RallyAPI.Users.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add this BEFORE other service registrations
builder.Services.AddHttpContextAccessor();

// Add this for UsersDbContext
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UsersDb")));

// Add this for OrdersDbContext (if you have one)
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));


// Add Users Module
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddUsersEndpoints();

// Catalog Module
builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddCatalogEndpoints();

// Add ProRouting Integration
builder.Services.AddProRoutingIntegration(builder.Configuration);

// Add Order Module
builder.Services.AddOrdersModule(builder.Configuration);

// Add MediatR for Application layer
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RallyAPI.Users.Application.Abstractions.IUnitOfWork).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RallyAPI.Catalog.Application.Abstractions.IUnitOfWork).Assembly);
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Customer", policy =>
        policy.RequireClaim("user_type", "customer"));
    options.AddPolicy("Rider", policy =>
        policy.RequireClaim("user_type", "rider"));
    options.AddPolicy("Restaurant", policy =>
        policy.RequireClaim("user_type", "restaurant"));
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("user_type", "admin"));
});



// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add Global Exception Handler (early in pipeline!)
app.UseGlobalExceptionHandler();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapUsersEndpoints();
app.MapOrdersEndpoints();
app.MapGet("/", () => "Rally API is running!");

app.Run();

