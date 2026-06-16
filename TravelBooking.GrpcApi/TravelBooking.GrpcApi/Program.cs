using TravelBooking.GrpcApi.Services;
using TravelBooking.GrpcApi.Interceptors;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────
// gRPC + JSON Transcoding
// ─────────────────────────────────────────
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<LoggingInterceptor>();
    options.Interceptors.Add<ExceptionInterceptor>();
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
})
.AddJsonTranscoding();   // <-- This enables REST/JSON on top of gRPC

// ─────────────────────────────────────────
// Swagger for the JSON endpoints
// ─────────────────────────────────────────
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Travel Booking API",
        Version = "v1",
        Description = "gRPC + REST/JSON API for flight and hotel search, pricing, and booking"
    });
});

// ─────────────────────────────────────────
// Logging
// ─────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ─────────────────────────────────────────
// Middleware
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Booking API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();

// ─────────────────────────────────────────
// Map gRPC Service
// ─────────────────────────────────────────
app.MapGrpcService<TravelService>();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/", () => "Travel Booking gRPC API is running. Visit /swagger for REST docs.");

app.Run();
