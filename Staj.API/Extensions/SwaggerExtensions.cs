// Staj.API/Extensions/SwaggerExtensions.cs
using Microsoft.OpenApi;

namespace Staj.API.Extensions;

// Swagger yapılandırması
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Staj API",
                Version = "v1",
                Description = "Mühendis & İşveren Eşleştirme Platformu API"
            });

            // JWT Bearer güvenlik tanımı
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT token giriniz. Örnek: Bearer eyJ...",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // Tüm endpoint'lere Bearer requirement ekle
            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
{
    {
        new OpenApiSecuritySchemeReference("Bearer", document),
        new List<string>()
    }
});
        });

        return services;
    }
}