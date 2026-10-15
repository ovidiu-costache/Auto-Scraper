using Api.Endpoints;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is required. Configure ConnectionStrings:DefaultConnection in the Api project's User Secrets for local development.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<DbServices>();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        // Write JSON even when a browser requests an HTML error response.
        IResult result = exception is BadHttpRequestException { StatusCode: 400 } badRequest
            ? Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["query"] = [badRequest.Message]
            }, title: "Invalid query parameters.")
            : Results.Problem(statusCode: exception is BadHttpRequestException error
                ? error.StatusCode
                : StatusCodes.Status500InternalServerError);

        await result.ExecuteAsync(context);
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "AutoScraper API v1"));
}

app.UseHttpsRedirection();
app.MapCarListingEndpoints();

app.Run();

public partial class Program
{
}
