using Bookify.Api.Extensions;
using Bookify.Api.OpenApi;
using Bookify.Application;
using Bookify.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // REMARK: Uncomment if you want to use Swagger (also need to install needed libraries)
    // app.UseSwagger();
    // app.UseSwaggerUI(options =>
    // {
    //     var descriptions = app.DescribeApiVersions();
    //
    //     foreach (var description in descriptions)
    //     {
    //         var url = $"/swagger/{description.GroupName}/swagger.json";
    //         var name = description.GroupName.ToUpperInvariant();
    //         options.SwaggerEndpoint(url, name);
    //     }
    // });

    app.MapOpenApi();
    app.ApplyMigrations();

    // REMARK: Uncomment if to seed initial data.
    // app.SeedData();
}

// app.UseHttpsRedirection();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseCustomExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Configure minimal APIs with versioning
// var apiVersionSet = app.NewApiVersionSet()
//     .HasApiVersion(new ApiVersion(1))
//     .ReportApiVersions()
//     .Build();
//
// var routeGroupBuilder = app.MapGroup("api/v{version:apiVersion}").WithApiVersionSet(apiVersionSet);
//
// routeGroupBuilder.MapBookingEndpoints();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

public partial class Program;