using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bookify.Infrastructure.Authentication;

internal sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _authenticationOptions;

    public JwtBearerOptionsSetup(IOptions<AuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _authenticationOptions.Audience;
        options.MetadataAddress = _authenticationOptions.MetadataUrl;
        options.RequireHttpsMetadata = _authenticationOptions.RequireHttpsMetadata;
        options.TokenValidationParameters.ValidIssuer = _authenticationOptions.Issuer;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Prevent default response
                context.HandleResponse();

                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Type = "Unauthorized Error",
                    Title = "Unauthorized",
                    Detail = "Authentication is required to access this resource."
                };

                context.Response.StatusCode = problemDetails.Status.Value;
                context.Response.ContentType = "application/problem+json";
                var json = JsonSerializer.Serialize(problemDetails);
                return context.Response.WriteAsync(json);
            },
            OnForbidden = context =>
            {
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Type = "ForbiddenError",
                    Title = "Forbidden",
                    Detail = "You do not have permission to access this resource."
                };

                context.Response.StatusCode = problemDetails.Status.Value;
                context.Response.ContentType = "application/problem+json";
                var json = JsonSerializer.Serialize(problemDetails);
                return context.Response.WriteAsync(json);
            }
        };
    }
}