using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Diagnostics;

namespace TravelBooking.GrpcApi.Interceptors;

/// <summary>
/// Logs all gRPC calls with timing information.
/// </summary>
public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var method  = context.Method;
        var sw      = Stopwatch.StartNew();

        _logger.LogInformation("gRPC call started: {Method}", method);

        try
        {
            var response = await continuation(request, context);
            sw.Stop();
            _logger.LogInformation("gRPC call completed: {Method} in {ElapsedMs}ms", method, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "gRPC call failed: {Method} in {ElapsedMs}ms", method, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
