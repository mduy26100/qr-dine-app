using QRDine.API.Constants;
using QRDine.API.DependencyInjection;
using QRDine.API.Filters;
using QRDine.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ===== Service Registration =====
builder.Services
    .AddControllers(options => options.Filters.Add<ApiResponseFilter>())
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc(SwaggerGroups.Admin, new OpenApiInfo
        {
            Title = "QRDine Admin API",
            Version = "v1"
        });

        options.SwaggerDoc(SwaggerGroups.Management, new OpenApiInfo
        {
            Title = "QRDine Management API",
            Version = "v1"
        });

        options.SwaggerDoc(SwaggerGroups.Storefront, new OpenApiInfo
        {
            Title = "QRDine Storefront API",
            Version = "v1"
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

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SubscriptionEnforcementMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
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

await app.SeedDataAsync();

app.Run();
