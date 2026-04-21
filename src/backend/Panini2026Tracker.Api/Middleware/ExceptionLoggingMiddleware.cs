using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Api.Middleware;

public sealed class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            using var scope = scopeFactory.CreateScope();
            var logRepository = scope.ServiceProvider.GetRequiredService<ISystemLogRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await logRepository.AddAsync(
                new SystemLog(
                    "system",
                    "request.unhandled_exception",
                    $"Unhandled exception at {context.Request.Method} {context.Request.Path}: {exception.GetBaseException().Message}",
                    "error",
                    DateTime.UtcNow),
                context.RequestAborted);

            await unitOfWork.SaveChangesAsync(context.RequestAborted);
            throw;
        }
    }
}
