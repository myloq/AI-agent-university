using System.Text.Json;
using AiAgentUniversity.Domain.Interfaces;
using AiAgentUniversity.Domain.Models;

namespace AiAgentUniversity.Data.Storage;

public class JsonStorage : IStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _path;

    public JsonStorage(string path) => _path = path;

    public List<Product> Load()
    {
        // Первый запуск: если файла нет - возвращаем пустой список
        if (!File.Exists(_path)) return new List<Product>();

        try
        {
            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json)) return new List<Product>();

            return JsonSerializer.Deserialize<List<Product>>(json, Options) ?? new List<Product>();
        }
        catch (JsonException)
        {
            Console.WriteLine($"[Предупреждение] Файл {_path} поврежден. Начинаем с пустого списка.");
            return new List<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Ошибка] Не удалось прочитать файл {_path}: {ex.Message}");
            return new List<Product>();
        }
    }

    public void Save(List<Product> products)
    {
        try
        {
            var json = JsonSerializer.Serialize(products, Options);
            File.WriteAllText(_path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Ошибка] Не удалось сохранить данные: {ex.Message}");
        }
    }
}
