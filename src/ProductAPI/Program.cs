using ProductAPI.Models;
using ProductAPI.Repositories;

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

app.UseHttpsRedirection();
app.MapControllers();

// Endpoint de health check
app.MapGet("/api/products/health", () => 
    Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithName("HealthCheck");

app.Run();
