// File: src/Modules/Orders/RallyAPI.Orders.Application/Commands/ProcessPayuWebhook/ProcessPayuWebhookCommand.cs

using MediatR;
using RallyAPI.SharedKernel;
using RallyAPI.SharedKernel.Results;

namespace RallyAPI.Orders.Application.Commands.ProcessPayuWebhook;

public record ProcessPayuWebhookCommand(
    Dictionary<string, string> FormData
) : IRequest<Result<bool>>;