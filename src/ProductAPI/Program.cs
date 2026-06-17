using ProductAPI.Models;
using ProductAPI.Repositories;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddControllers();
// Habilitar generación de OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Exponer OpenAPI y Swagger UI siempre
app.UseSwagger();
app.UseSwaggerUI();

// Registrar metricas HTTP de Prometheus
app.UseHttpMetrics();

app.UseHttpsRedirection();
app.MapControllers();

// Endpoint de health check
app.MapGet("/api/products/health", () => 
    Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithName("HealthCheck");

// Exponer endpoint /metrics de Prometheus
app.MapMetrics();

app.Run();
