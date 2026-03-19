using QRDine.API.Constants;
using QRDine.API.DependencyInjection;
using QRDine.API.Filters;
using QRDine.API.Middlewares;
using QRDine.Infrastructure.SignalR.Hubs;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

var builder = WebApplication.CreateBuilder(args);

// ===== Service Registration =====
builder.Services
    .AddControllers(options => options.Filters.Add<ApiResponseFilter>())
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc(SwaggerGroups.Identity, new OpenApiInfo
        {
            Title = "QRDine Identity API",
            Version = "v1",
            Description = "API dùng cho xác thực và quản lý danh tính người dùng như đăng ký, đăng nhập, kích hoạt tài khoản, và cấp token."
        });

        options.SwaggerDoc(SwaggerGroups.Admin, new OpenApiInfo
        {
            Title = "QRDine Admin API",
            Version = "v1",
            Description = "API dành cho quản trị viên hệ thống để quản lý merchant, người dùng, cấu hình hệ thống và giám sát hoạt động."
        });

        options.SwaggerDoc(SwaggerGroups.Management, new OpenApiInfo
        {
            Title = "QRDine Management API",
            Version = "v1",
            Description = "API dành cho chủ cửa hàng hoặc nhân viên quản lý để quản lý menu, sản phẩm, bàn, đơn hàng và báo cáo."
        });

        options.SwaggerDoc(SwaggerGroups.Storefront, new OpenApiInfo
        {
            Title = "QRDine Storefront API",
            Version = "v1",
            Description = "API dành cho phía khách hàng khi quét QR để xem menu, thêm món vào giỏ, đặt món và theo dõi trạng thái đơn hàng."
        });

        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            return apiDesc.GroupName == docName;
        });
    })
    .AddHttpClient()
    .AddApplicationServices(builder.Configuration);


var app = builder.Build();

app.UseForwardedHeaders();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<StorefrontSubscriptionMiddleware>();
app.UseAuthorization();

app.MapHub<OrderHub>("/hubs/order");

app.UseMiddleware<SubscriptionEnforcementMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            $"/swagger/{SwaggerGroups.Identity}/swagger.json",
            "Identity API");

        options.SwaggerEndpoint(
            $"/swagger/{SwaggerGroups.Admin}/swagger.json",
            "Admin API");

        options.SwaggerEndpoint(
            $"/swagger/{SwaggerGroups.Management}/swagger.json",
            "Management API");

        options.SwaggerEndpoint(
            $"/swagger/{SwaggerGroups.Storefront}/swagger.json",
            "Storefront API");
    });
}

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.SeedDataAsync();

app.Run();
