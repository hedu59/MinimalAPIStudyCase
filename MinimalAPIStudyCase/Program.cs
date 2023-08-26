using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MinimalAPIStudyCase.Data;
using MinimalAPIStudyCase.Models;
using MiniValidation;
using NetDevPack.Identity;
using NetDevPack.Identity.Jwt;
using NetDevPack.Identity.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("MinimalAPIStudyCase")));

builder.Services.AddDbContext<ContextDb>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DeleteToy", policy => policy.RequireClaim("DeleteToy"));
});

builder.Services.AddEndpointsApiExplorer();
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthConfiguration();
app.UseHttpsRedirection();


#region USER

//UserRegister
app.MapPost("/register", [AllowAnonymous] async (
       SignInManager<IdentityUser> signInManager,
       UserManager<IdentityUser> userManager,
       IOptions<AppJwtSettings> appJwtSettings,
       RegisterUser registerUser) =>
{
    return await RegisterUserService(userManager, appJwtSettings, registerUser);

}).ProducesValidationProblem()
     .Produces(StatusCodes.Status200OK)
     .Produces(StatusCodes.Status400BadRequest)
     .WithName("UserRegister")
     .WithTags("Users");


//UserLogin
app.MapPost("/login", [AllowAnonymous] async (
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOptions<AppJwtSettings> appJwtSettings,
        LoginUser loginUser) =>
{
    if (loginUser == null)
        return Results.BadRequest("User must be informed");

    if (!MiniValidator.TryValidate(loginUser, out var errors))
        return Results.ValidationProblem(errors);

    var result = await signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

    if (result.IsLockedOut)
        return Results.BadRequest("User was blocked");

    if (!result.Succeeded)
        return Results.BadRequest("User or Password invalid");

    var jwt = new JwtBuilder()
                .WithUserManager(userManager)
                .WithJwtSettings(appJwtSettings.Value)
                .WithEmail(loginUser.Email)
                .WithJwtClaims()
                .WithUserClaims()
                .WithUserRoles()
                .BuildUserResponse();

    return Results.Ok(jwt);

}).ProducesValidationProblem()
      .Produces(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("UserLogin")
      .WithTags("Users");



//Methods

static async Task<IResult> RegisterUserService(UserManager<IdentityUser> userManager, IOptions<AppJwtSettings> appJwtSettings, RegisterUser registerUser)
{
    if (registerUser == null)
        return Results.BadRequest("User must be informed");

    if (!MiniValidator.TryValidate(registerUser, out var errors))
        return Results.ValidationProblem(errors);

    var user = new IdentityUser
    {
        UserName = registerUser.Email,
        Email = registerUser.Email,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, registerUser.Password);

    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    var jwt = new JwtBuilder()
                .WithUserManager(userManager)
                .WithJwtSettings(appJwtSettings.Value)
                .WithEmail(user.Email)
                .WithJwtClaims()
                .WithUserClaims()
                .WithUserRoles()
                .BuildUserResponse();

    return Results.Ok(jwt);
}

#endregion

#region TOYS
//GetAll
app.MapGet("/toy",[Authorize] async (ContextDb context) =>
    await context.Toys.ToListAsync())
.Produces<Toy>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetToy")
.WithTags("Toys");


//GetById
app.MapGet("/toy/{id}", [Authorize] async (ContextDb context, Guid id) =>
    await context.Toys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) is Toy toy
    ? Results.Ok(toy)
    : Results.NotFound())
.Produces<Toy>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetToyById")
.WithTags("Toys");


//PostToy
app.MapPost("/toy", [Authorize]  async (ContextDb context, ToyCommand toyCommand) =>
{
    if (!MiniValidator.TryValidate(toyCommand, out var erros))
        return Results.ValidationProblem(erros);

    var toy = Toy.Converter(toyCommand);
    context.Toys.Add(toy);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.Created($"/toy/{toy.Id}", toy)
    : Results.BadRequest("Something went wrong");
})
.ProducesValidationProblem()
.Produces<Toy>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostToy")
.WithTags("Toys");


//PutToy
app.MapPut("/toy/{id}", [Authorize] async (ContextDb context, ToyCommand toyCommand, Guid id) =>
{
    var toyDataBase = await context.Toys.AsNoTracking<Toy>().FirstOrDefaultAsync(x => x.Id == id);
    if (toyDataBase == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(toyCommand, out var erros))
        return Results.ValidationProblem(erros);

    var toy = Toy.Converter(toyCommand);
    context.Toys.Update(toy);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.NoContent()
    : Results.BadRequest("Something went wrong");
})
.ProducesValidationProblem()
.Produces<Toy>(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PutToy")
.WithTags("Toys");


//DeleteToy
app.MapDelete("/toy/{id}", [Authorize] async (Guid id, ContextDb context) =>
{
    var toyDataBase = await context.Toys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    if (toyDataBase == null) return Results.NotFound();

    context.Toys.Remove(toyDataBase);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.NoContent()
    : Results.BadRequest("Something went wrong");
})

.Produces(StatusCodes.Status403Forbidden)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.RequireAuthorization("DeleteToy")
.WithName("DeleteToys")
.WithTags("Toys");
#endregion

app.Run();

