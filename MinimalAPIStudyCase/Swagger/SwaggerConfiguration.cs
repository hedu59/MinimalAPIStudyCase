using Microsoft.OpenApi.Models;

namespace MinimalAPIStudyCase.Swagger
{
    public static class SwaggerConfiguration
    {
        public static void SwaggerConfigurationBuilder(this WebApplicationBuilder builder)
        {

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MinimalAPIStudyCase",
                    Description = "Developed by Carlos Eduardo follwing Eduardo Pires tutorial - Owner @desenvolvedor.io",
                    Contact = new OpenApiContact { Name = "Carlos Eduardo", Email = "59carloseduardo@gmail.com" }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insert the JWT token using this way: Bearer {your token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }
    }
}
