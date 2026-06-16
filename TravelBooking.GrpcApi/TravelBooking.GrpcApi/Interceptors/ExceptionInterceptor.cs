using Grpc.Core;
using Grpc.Core.Interceptors;

namespace TravelBooking.GrpcApi.Interceptors;

/// <summary>
/// Converts unhandled exceptions into proper gRPC status codes.
/// JSON transcoding maps these to HTTP status codes automatically:
///   INVALID_ARGUMENT  → 400
///   NOT_FOUND         → 404
///   ALREADY_EXISTS    → 409
///   INTERNAL          → 500
/// </summary>
public class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            // Already a proper gRPC exception — let it propagate
            throw;
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in gRPC handler");
            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred"));
        }
    }
}
