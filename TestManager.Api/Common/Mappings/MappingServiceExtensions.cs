namespace TestManager.Api.Common.Mappings;

public static class MappingServiceExtensions
{
    public static IServiceCollection AddMappings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg =>
        {
            var licenseKey = configuration["AutoMapper:LicenseKey"];
            if (!string.IsNullOrWhiteSpace(licenseKey))
                cfg.LicenseKey = licenseKey;
        }, typeof(MappingServiceExtensions).Assembly);

        return services;
    }
}
