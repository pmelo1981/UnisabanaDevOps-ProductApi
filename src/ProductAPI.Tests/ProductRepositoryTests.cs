using Xunit;
using ProductAPI.Models;
using ProductAPI.Repositories;

namespace ProductAPI.Tests;

public class ProductRepositoryTests
{
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _repository = new ProductRepository();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProducts()
    {
        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Act
        var product = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var product = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(product);
    }

    [Fact]
    public async Task AddAsync_CreatesNewProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        var created = await _repository.AddAsync(newProduct);

        // Assert
        Assert.NotNull(created);
        Assert.NotEqual(0, created.Id);
        Assert.Equal("Test Product", created.Name);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesProduct()
    {
        // Arrange
        var existing = await _repository.GetByIdAsync(1);
        var updatedProduct = new Product
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 199.99m,
            Stock = 20
        };

        // Act
        var result = await _repository.UpdateAsync(1, updatedProduct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(199.99m, result.Price);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        // Arrange
        var product = await _repository.AddAsync(new Product
        {
            Name = "To Delete",
            Description = "Test",
            Price = 50m,
            Stock = 5
        });

        // Act
        var deleted = await _repository.DeleteAsync(product.Id);
        var retrieved = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }
}
