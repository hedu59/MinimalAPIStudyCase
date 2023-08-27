using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MiniValidation;
using NetDevPack.Identity.Jwt;
using NetDevPack.Identity.Model;

namespace MinimalAPIStudyCase.Endpoints
{
    public static class UserEndpoint
    {
        public static void UserEndpointConfiguration(this WebApplication app)
        {
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

        }
    }
}
