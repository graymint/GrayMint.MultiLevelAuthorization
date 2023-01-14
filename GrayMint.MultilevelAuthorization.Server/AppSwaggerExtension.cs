using System.Net;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MultiLevelAuthorization.Server;

internal static class AppSwaggerExtension
{
    public static IServiceCollection AddAppSwaggerGen(this IServiceCollection services, string productName)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = productName,
                    Version = "v1"
                });

            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                    },
                    Array.Empty<string>()
                }
            });

            // XML Documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            c.SchemaFilter<MySchemaFilter>();
            c.MapType<IPAddress>(() => new OpenApiSchema { Type = "string" });
            c.MapType<IPEndPoint>(() => new OpenApiSchema { Type = "string" });
            c.MapType<Version>(() => new OpenApiSchema { Type = "string" });
            c.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string" });
        });
        return services;
    }

    public class MySchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
        }
    }
}