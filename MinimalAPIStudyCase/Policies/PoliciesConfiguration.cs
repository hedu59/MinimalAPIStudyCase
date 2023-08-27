namespace MinimalAPIStudyCase.Policies
{
    public static class PoliciesConfiguration
    {
        public static void PoliciesConfigurationBuilder(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteToy", policy => policy.RequireClaim("DeleteToy"));
            });
        }
    }
}
