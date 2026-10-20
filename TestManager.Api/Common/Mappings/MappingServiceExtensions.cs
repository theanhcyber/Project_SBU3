namespace TestManager.Api.Common.Mappings;

public static class MappingServiceExtensions
{
    // Quét toàn bộ AutoMapper Profile (UserMapping, ClassRoomMapping, ExamMapping...) và đăng ký IMapper dùng chung
    public static IServiceCollection AddMappings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg =>
        {
            // AutoMapper >= 15 cần license key (bản Community miễn phí: https://luckypennysoftware.com).
            // Lấy từ appsettings "AutoMapper:LicenseKey" hoặc biến môi trường AUTOMAPPER_LICENSE_KEY.
            // Không có key vẫn chạy bình thường, chỉ ghi cảnh báo ra log.
            var licenseKey = configuration["AutoMapper:LicenseKey"];
            if (!string.IsNullOrWhiteSpace(licenseKey))
                cfg.LicenseKey = licenseKey;
        }, typeof(MappingServiceExtensions).Assembly);

        return services;
    }
}
