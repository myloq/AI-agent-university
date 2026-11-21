using AiAgentUniversity.Data.Storage;
using AiAgentUniversity.Domain.Models;

namespace AiAgentUniversity.Tests;

public class JsonStorageTests
{
    [Fact]
    public void Load_FileDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        var storage = new JsonStorage("non_existent_file_12345.json");

        // Act
        var result = storage.Load();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SaveAndLoad_ValidData_ReturnsSavedData()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var storage = new JsonStorage(tempFile);
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Тестовый продукт", Quantity = 1.5m, Unit = "кг", Category = "тест", ExpiryDate = DateTime.Today }
        };

        try
        {
            // Act
            storage.Save(products);
            var loaded = storage.Load();

            // Assert
            Assert.Single(loaded);
            Assert.Equal("Тестовый продукт", loaded[0].Name);
            Assert.Equal(1.5m, loaded[0].Quantity);
        }
        finally
        {
            // Уборка: удаляем временный файл после теста
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_CorruptedJson_ReturnsEmptyList()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "{ invalid json }"); // Ломаем JSON
        var storage = new JsonStorage(tempFile);

        try
        {
            // Act
            var loaded = storage.Load();

            // Assert
            Assert.Empty(loaded); // Должен сработать catch и вернуться пустой список
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_ThrowsGeneralException_ReturnsEmptyList()
    {
        // Arrange: передаем путь к папке вместо файла, чтобы вызвать ошибку доступа (IOException/UnauthorizedAccessException)
        var storage = new JsonStorage(Path.GetTempPath());

        // Act
        var result = storage.Load();

        // Assert
        Assert.Empty(result); // Должен сработать catch (Exception) и вернуться пустой список
    }

    [Fact]
    public void Save_ThrowsGeneralException_DoesNotCrash()
    {
        // Arrange
        var storage = new JsonStorage(Path.GetTempPath());
        var products = new List<Product> { new Product { Name = "Test" } };

        // Act (программа не должна упасть, просто выведет ошибку в консоль)
        var exception = Record.Exception(() => storage.Save(products));

        // Assert
        Assert.Null(exception); // Исключение поймано внутри Save
    }
}
