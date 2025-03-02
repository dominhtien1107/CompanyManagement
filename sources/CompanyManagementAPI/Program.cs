using CompanyManagementAPI.Extensions;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Thêm CORS và IIS vào phần cấu hình dịch vụ.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();

builder.Services.AddControllers(); // Đăng ký các Controllers trong IServiceCollection (Không sử dụng Views).

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Tạo biến ứng dụng của lớp WebApplication.
// Triển khai nhiều giao diện như: IHost, IApplicationBuilder, IEndpointRouteBuilder, ...
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
else
{
    app.UseHsts(); // HTTP Strict Transport Security: tính năng bảo mật cho website. Đảm bảo rằng tất cả kết nối tới một Web brower được mã hóa bằng giao thức HTTPS.
}

// Thứ tự thêm các Middlewares là RẤT QUAN TRỌNG.

app.UseHttpsRedirection(); // Chuyển hướng HTTP => HTTPS.

app.UseStaticFiles(); // Cho phép sử dụng Static Files trong ứng dụng (Default: wwwroot).

// Chuyển tiếp các tiêu đề Proxy => Request hiện tại.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All 
});

app.UseCors("CorsPolicy");

app.UseAuthorization(); // Uỷ quyền IApplicationBuilder đã chỉ định để kích hoạt khả năng ủy quyền. Phải nằm giữa app.UseRouting() và app.UseEndpoints(...) nếu cấp quyền.

app.MapControllers(); // Thêm các điểm cuối từ Controller Actions vào IEndpointRouteBuilder.

app.Run(); // Chạy ứng dụng và chặn luồng.