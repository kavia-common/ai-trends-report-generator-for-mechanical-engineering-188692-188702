using Backend.Controllers;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "AI Trends Report Generator API";
    settings.Description = "Backend API to fetch AI trends in mechanical engineering and generate .docx reports.";
    settings.Version = "1.0.0";
    settings.PostProcess = document =>
    {
        document.Tags = new[]
        {
            new NSwag.OpenApiTag { Name = "Trends", Description = "Endpoints for listing current AI trends." },
            new NSwag.OpenApiTag { Name = "Reports", Description = "Endpoints for generating and downloading reports." }
        };
    };
});

// Dependency Injection
builder.Services.AddSingleton<ITrendsService, TrendsService>();
builder.Services.AddSingleton<IReportService, ReportService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

// Configure OpenAPI/Swagger
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.Path = "/docs";
});

// Health check endpoint
app.MapGet("/", () => new { message = "Healthy" });

// Map API endpoints
app.MapTrendsEndpoints();
app.MapReportsEndpoints();

app.Run();