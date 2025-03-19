using Contracts;
using LoggerService;
using Repository;
using Service.Contracts;
using Service;
using Microsoft.EntityFrameworkCore;

namespace CompanyManagementAPI.Extensions;

// 1.4 Extension methods and CORS (Cross-Origin Resource Sharing) configuration.

// Cơ chế cấp hoặc hạn chế quyền truy cập vào ứng dụng từ các "Domain" khác nhau.
// Nếu muốn gửi Request từ một Domain khác vào ứng dụng => Cấu hình này là bắt buộc.
public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services) => services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin() // Cho phép truy cập từ tất cả các nguồn <=> WithOrigins(): Chỉ định một nguồn cụ thể.
        .AllowAnyMethod() // Cho phép tất cả các phương thức HTTP <=> WithMethods(): Chỉ định các phương thức cụ thể.
        .AllowAnyHeader() // <=> WithHeaders(): Chỉ định các tiêu đề cụ thể.
        .WithExposedHeaders("X-Pagination"));
    });

    // 1.5 IIS Configuration
    // Cấu hình tích hợp IIS
    public static void ConfigureIISIntegration(this IServiceCollection services) =>
        services.Configure<IISOptions>(options =>
        { 
            // Thêm các thuộc tính tại đây.
            // Hiện tại chưa sử dụng đến.

            //options.AutomaticAuthentication = true;
            //options.AuthenticationDisplayName = "";
            //options.ForwardClientCertificate = true;
        });

    public static void ConfigureLoggerService(this IServiceCollection services) => services.AddSingleton<ILoggerManager, LoggerManager>();
    public static void ConfigureRepositoryManager(this IServiceCollection services) => services.AddScoped<IRepositoryManager, RepositoryManager>();
    public static void ConfigureServiceManager(this IServiceCollection services) => services.AddScoped<IServiceManager, ServiceManager>();
    public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<RepositoryContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("sqlConnection")));
    public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) => builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));
}
