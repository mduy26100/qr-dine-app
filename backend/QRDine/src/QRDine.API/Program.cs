using QRDine.API.DependencyInjection;
using QRDine.API.Filters;
using QRDine.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ===== Service Registration =====
builder.Services
    .AddControllers(options => options.Filters.Add<ApiResponseFilter>())
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddHttpClient()
    .AddApplicationServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//Seed data on startup
await app.SeedDataAsync();

app.Run();
