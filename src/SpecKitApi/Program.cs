using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;
using SpecKitApi.Endpoints;
using SpecKitApi.Extensions;
using SpecKitApi.Middleware;
using SpecKitApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddJsonPlaceholderServices(builder.Configuration);
builder.Services.AddOpenApi("v1");

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async ctx =>
    {
        var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is { } ex)
        {
            logger.LogError(ex, "Unhandled exception");
        }

        var correlationId = ctx.Items["CorrelationId"]?.ToString() ?? ctx.TraceIdentifier;
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        var error = new ErrorResponse(
            "An unexpected error occurred.",
            "INTERNAL_ERROR",
            correlationId
        );
        await ctx.Response.WriteAsync(
            JsonSerializer.Serialize(
                error,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            )
        );
    });
});

app.MapAlbums();

app.Run();

public partial class Program { }
