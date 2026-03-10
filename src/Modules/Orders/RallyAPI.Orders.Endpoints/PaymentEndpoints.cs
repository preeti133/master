// File: src/Modules/Orders/RallyAPI.Orders.Endpoints/PaymentEndpoints.cs
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RallyAPI.Orders.Application.Commands.InitiatePayment;
using RallyAPI.Orders.Application.Commands.ProcessPayuWebhook;
using RallyAPI.Orders.Application.Commands.RefundPayment;
using RallyAPI.Orders.Application.Commands.VerifyPayment;

namespace RallyAPI.Orders.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments");

        // 1. Initiate payment — returns PayU checkout params
        group.MapPost("/initiate", async (
            InitiatePaymentRequest request,
            HttpContext httpContext,
            ISender sender) =>
        {
            var customerId = Guid.Parse(httpContext.User.FindFirst("sub")?.Value ?? "");

            var result = await sender.Send(new InitiatePaymentCommand(
                request.OrderId,
                customerId));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .RequireAuthorization("Customer")
        .RequireRateLimiting("login")  // reuse login rate limit (5/15min)
        .WithName("InitiatePayment");

        // 2. PayU webhook (S2S callback) — source of truth
        group.MapPost("/webhook", async (
            HttpContext httpContext,
            ISender sender) =>
        {
            // PayU sends form-urlencoded POST
            var form = await httpContext.Request.ReadFormAsync();
            var formData = form.ToDictionary(
                x => x.Key,
                x => x.Value.ToString());

            var result = await sender.Send(new ProcessPayuWebhookCommand(formData));

            // Always return 200 to PayU — even on failure, to prevent retries
            return Results.Ok();
        })
        .AllowAnonymous()  // PayU server-to-server, no JWT
        .WithName("PayUWebhook");

        // 3. Verify payment — frontend backup check
        group.MapPost("/verify", async (
            VerifyPaymentRequest request,
            HttpContext httpContext,
            ISender sender) =>
        {
            var customerId = Guid.Parse(httpContext.User.FindFirst("sub")?.Value ?? "");

            var result = await sender.Send(new VerifyPaymentCommand(
                request.TxnId,
                customerId));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .RequireAuthorization("Customer")
        .WithName("VerifyPayment");

        // 4. Refund — admin only
        group.MapPost("/refund", async (
            RefundPaymentRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(new RefundPaymentCommand(
                request.OrderId,
                request.Amount));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .RequireAuthorization("Admin")
        .WithName("RefundPayment");

        // 5. Success/Failure return URLs (PayU redirects browser here)
        group.MapPost("/return/success", (HttpContext ctx) =>
        {
            // PayU redirects here after successful payment.
            // For web: redirect to your frontend success page.
            // The form data contains the same fields as the webhook.
            return Results.Redirect("/payment-success");
        })
        .AllowAnonymous()
        .WithName("PaymentReturnSuccess");

        group.MapPost("/return/failure", (HttpContext ctx) =>
        {
            return Results.Redirect("/payment-failed");
        })
        .AllowAnonymous()
        .WithName("PaymentReturnFailure");


        return app;  
    }
}

// === Request DTOs ===

public record InitiatePaymentRequest(Guid OrderId);
public record VerifyPaymentRequest(string TxnId);
public record RefundPaymentRequest(Guid OrderId, decimal? Amount);