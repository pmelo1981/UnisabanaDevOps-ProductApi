using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Controllers;
using ProductAPI.Models;
using ProductAPI.Repositories;

namespace ProductAPI.Tests;

public class ProductsControllerTests
{
    private readonly ProductRepository _repository;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _repository = new ProductRepository();
        _controller = new ProductsController(_repository);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        // Arrange & Act
        var result = await _controller.GetAll();

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult()
    {
        // Arrange & Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange & Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.NotNull(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Nuevo Producto",
            Description = "Prueba",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        var result = await _controller.Create(newProduct);

        // Assert
        Assert.NotNull(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProductsController.GetById), createdResult.ActionName);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Name = "Actualizado",
            Description = "Descripción actualizada",
            Price = 199.99m,
            Stock = 20
        };

        // Act
        var result = await _controller.Update(1, updatedProduct);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange & Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange & Act
        var result = await _controller.Delete(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetStats_ReturnsOkResult()
    {
        // Arrange & Act
        var result = await _controller.GetStats();

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }
}
