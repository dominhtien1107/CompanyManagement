using CompanyManagementAPI.Extensions;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using NLog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));



// Thêm CORS và IIS vào phần cấu hình dịch vụ.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers(config => { 
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
}).AddXmlDataContractSerializerFormatters()
.AddCustomCSVFormatter()
.AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);

builder.Services.AddControllers().AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly); // Đăng ký các Controllers trong IServiceCollection (Không sử dụng Views).

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Tạo biến ứng dụng của lớp WebApplication.    
// Triển khai nhiều giao diện như: IHost, IApplicationBuilder, IEndpointRouteBuilder, ...
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);
if (app.Environment.IsProduction())
    app.UseHsts();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
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

//app.Use(async (context, next) =>
//{
//    await context.Response.WriteAsync("Hello from the middleware component.");
//    //Console.WriteLine($"Logic before executing the next delegate in the Use method"); 
//    await next.Invoke();
//    Console.WriteLine($"Logic after executing the next delegate in the Use method");
//});

//app.Map("/usingmapbranch", builder =>
//{
//    builder.Use(async (context, next) =>
//    {
//        Console.WriteLine("Map branch logic in the Use method before the next delegate");
//        await next.Invoke(); Console.WriteLine("Map branch logic in the Use method after the next delegate");
//    });

//    builder.Run(async context =>
//    {
//        Console.WriteLine($"Map branch response to the client in the Run method");
//        await context.Response.WriteAsync("Hello from the map branch.");
//    });
// });

//app.MapWhen(context => context.Request.Query.ContainsKey("testquerystring"), builder =>
//{
//    builder.Run(async context =>
//    {
//        await context.Response.WriteAsync("Hello from the MapWhen branch.");
//    });
//});

//app.Run(async context => 
//{
//    Console.WriteLine("Writing the response to the client in the Run method");
//    await context.Response.WriteAsync("Hello from the middleware component."); 
//});

app.MapControllers(); // Thêm các điểm cuối từ Controller Actions vào IEndpointRouteBuilder.

app.Run(); // Chạy ứng dụng và chặn luồng.