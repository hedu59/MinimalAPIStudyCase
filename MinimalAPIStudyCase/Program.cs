using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using MinimalAPIStudyCase.Data;
using MinimalAPIStudyCase.Endpoints;
using MinimalAPIStudyCase.Policies;
using MinimalAPIStudyCase.Swagger;
using NetDevPack.Identity.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("MinimalAPIStudyCase")));

builder.Services.AddDbContext<ContextDb>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

builder.PoliciesConfigurationBuilder();

builder.Services.AddEndpointsApiExplorer();
builder.SwaggerConfigurationBuilder();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

#region EndPoints

app.UserEndpointConfiguration();
app.ToyEndpointConfiguration();

#endregion

app.UseHttpsRedirection();
app.Run();

