﻿using AspNetCoreRateLimit;
using CompanyEmployees.Presentation.ActionFilters;
using CompanyManagementAPI.Extensions;
using CompanyManagementAPI.Utility;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;
using Service.DataShaping;
using Shared.DataTransferObjects;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() =>
    new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson().
    Services.BuildServiceProvider().
    GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters
    .OfType<NewtonsoftJsonPatchInputFormatter>().First();


// Thêm CORS và IIS vào phần cấu hình dịch vụ.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.ConfigureVersioning();
builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeaders();
builder.Services.AddMemoryCache();
builder.Services.ConfigureRateLimitingOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.ConfigureSwagger();

builder.Services.AddControllers(config => { 
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
}).AddXmlDataContractSerializerFormatters()
.AddCustomCSVFormatter()
.AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);

builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

builder.Services.AddControllers().AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly); // Đăng ký các Controllers trong IServiceCollection (Không sử dụng Views).
builder.Services.AddCustomMediaTypes();

builder.Services.AddScoped<ActionFilterExample>();
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();
builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();

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
    app.UseSwagger();
    app.UseSwaggerUI(s =>
    {
        s.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Maze API v1");
        s.SwaggerEndpoint("/swagger/v2/swagger.json", "Code Maze API v2");
    });
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

app.UseIpRateLimiting();
app.UseCors("CorsPolicy");
app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseAuthentication();
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