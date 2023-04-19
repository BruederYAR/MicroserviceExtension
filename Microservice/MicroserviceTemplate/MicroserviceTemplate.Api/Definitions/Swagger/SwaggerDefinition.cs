using MicroserviceTemplate.Api.Definitions.Options.Models;
using MicroserviceTemplate.Base.Attributes;
using MicroserviceTemplate.Base.Definition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MicroserviceTemplate.Api.Definitions.Swagger;

/// <summary>
/// Swagger definition for application
/// </summary>
public class SwaggerDefinition : Definition
{
    private const string _appVersion = "1.0.0";
    private const string _swaggerConfig = "/swagger/v1/swagger.json";

    public override void ConfigureApplicationAsync(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using var scope = app.Services.CreateAsyncScope();
        var url = scope.ServiceProvider.GetService<IOptions<IdentityAddressOption>>()!.Value.Authority;
        var client = scope.ServiceProvider.GetService<IOptions<IdentityClientOption>>()!.Value;

        
        app.UseSwagger();
        app.UseSwaggerUI(settings =>
        {
            settings.DefaultModelExpandDepth(0);
            settings.DefaultModelRendering(ModelRendering.Model);
            settings.DefaultModelsExpandDepth(0);
            settings.DocExpansion(DocExpansion.None);
            settings.OAuthScopeSeparator(" ");
            settings.OAuthClientId(client.Id);
            settings.OAuthClientSecret(client.Secret);
            settings.DisplayRequestDuration();
        });
    }

    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Microservice Template",
                Version = _appVersion,
                Description = "Microservice Template"
            });

            options.ResolveConflictingActions(x => x.First());

            options.TagActionsBy(api =>
            {
                string tag;
                if (api.ActionDescriptor is { } descriptor)
                {
                    var attribute = descriptor.EndpointMetadata.OfType<FeatureGroupNameAttribute>().FirstOrDefault();
                    tag = attribute?.GroupName ?? descriptor.RouteValues["controller"] ?? "Untitled";
                }
                else
                {
                    tag = api.RelativePath!;
                }

                var tags = new List<string>();
                if (!string.IsNullOrEmpty(tag))
                {
                    tags.Add(tag);
                }
                return tags;
            });
            
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("identitysetting.json");
            IConfiguration identityConfiguration = configurationBuilder.Build();
            
            var url = identityConfiguration.GetSection("IdentityServerUrl").GetValue<string>("Authority");
            var currentClient = identityConfiguration.GetSection("CurrentIdentityClient").Get<IdentityClientOption>()!;

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{url}/connect/authorize", UriKind.Absolute),
                        TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                        Scopes = currentClient.Scopes!.ToDictionary(x => x, x=> x),
                    }
                }
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                    },
                    new List<string>()
                }
            });
        });
    }
}