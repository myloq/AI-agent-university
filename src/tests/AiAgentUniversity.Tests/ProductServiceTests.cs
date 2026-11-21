using AiAgentUniversity.Domain.Interfaces;
using AiAgentUniversity.Domain.Models;
using AiAgentUniversity.Domain.Services;

namespace AiAgentUniversity.Tests;

// Фейковое хранилище для тестов (изолирует тесты от реального файла data.json)
public class FakeStorage : IStorage
{
    public List<Product> StoredData { get; set; } = new();

    public List<Product> Load() => StoredData;
    public void Save(List<Product> products) => StoredData = products;
}

public class ProductServiceTests
{
    private readonly FakeStorage _fakeStorage;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        // Перед каждым тестом создаем чистое хранилище и сервис
        _fakeStorage = new FakeStorage();
        _service = new ProductService(_fakeStorage);
    }

    [Fact]
    public void Add_ShouldAddProduct_AndSaveToStorage()
    {
        // Act
        var product = _service.Add("Молоко", 2m, "л", "молочное", DateTime.Today.AddDays(5));

        // Assert
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
        Assert.Single(_service.All());
        Assert.Single(_fakeStorage.StoredData); // Проверяем, что данные ушли в хранилище
    }

    [Fact]
    public void Consume_ShouldDecreaseQuantity_AndSave()
    {
        // Arrange
        _service.Add("Хлеб", 2m, "шт", "хлеб", DateTime.Today);

        // Act
        var result = _service.Consume(1, 1m, out var message);

        // Assert
        Assert.True(result);
        Assert.Contains("Списано", message);
        Assert.Equal(1m, _service.FindById(1)!.Quantity);
        Assert.Equal(1m, _fakeStorage.StoredData[0].Quantity);
    }

    [Fact]
    public void Consume_ExactAmount_ShouldRemoveProduct()
    {
        // Arrange (проверяем сценарий с точностью, который ловил преподаватель)
        _service.Add("Специи", 0.0002m, "кг", "бакалея", DateTime.Today);

        // Act
        var result = _service.Consume(1, 0.0002m, out var message);

        // Assert
        Assert.True(result);
        Assert.Contains("полностью использован", message);
        Assert.Null(_service.FindById(1));
        Assert.Empty(_fakeStorage.StoredData); // Продукт удален и из хранилища
    }

    [Fact]
    public void Consume_MoreThanAvailable_ShouldReturnFalse_AndNotChangeState()
    {
        // Arrange
        _service.Add("Яйца", 10m, "шт", "молочное", DateTime.Today);

        // Act
        var result = _service.Consume(1, 15m, out var message);

        // Assert
        Assert.False(result);
        Assert.Contains("Нельзя списать", message);
        Assert.Equal(10m, _service.FindById(1)!.Quantity); // Остаток не изменился
    }

    [Fact]
    public void Consume_NonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = _service.Consume(99, 1m, out var message);

        // Assert
        Assert.False(result);
        Assert.Contains("не найден", message);
    }

    [Fact]
    public void ExpiringWithin_ShouldFilterAndSortCorrectly()
    {
        // Arrange
        _service.Add("Консервы", 5m, "шт", "бакалея", DateTime.Today.AddDays(365));
        _service.Add("Сыр", 1m, "кг", "молочное", DateTime.Today.AddDays(2));
        _service.Add("Йогурт", 2m, "шт", "молочное", DateTime.Today.AddDays(1));

        // Act
        var expiring = _service.ExpiringWithin(3);

        // Assert
        Assert.Equal(2, expiring.Count);
        Assert.Equal("Йогурт", expiring[0].Name); // Должен быть первым, так как истекает завтра
        Assert.Equal("Сыр", expiring[1].Name);
    }
}
