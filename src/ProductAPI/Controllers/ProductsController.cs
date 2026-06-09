using ProductAPI.Models;
using ProductAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ProductAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductRepository _repository;

    public ProductsController(ProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        // Returns product statistics: count, average, max, min prices
        var products = (await _repository.GetAllAsync()).ToList();
        if (!products.Any())
            return Ok(new { total = 0, promedio = 0, maximo = 0, minimo = 0 });

        return Ok(new
        {
            total = products.Count,
            promedio = Math.Round(products.Average(p => p.Price), 2),
            maximo = products.Max(p => p.Price),
            minimo = products.Min(p => p.Price)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        var created = await _repository.AddAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(int id, Product product)
    {
        var updated = await _repository.UpdateAsync(id, product);
        if (updated == null)
            return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (!result)
            return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });
        return NoContent();
    }
}
