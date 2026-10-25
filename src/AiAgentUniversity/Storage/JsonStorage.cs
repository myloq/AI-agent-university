using System.Text.Json;
using AiAgentUniversity.Models;

namespace AiAgentUniversity.Storage;

public class JsonStorage
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
        if (!File.Exists(_path)) return new List<Product>();
        try
        {
            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json)) return new List<Product>();
            return JsonSerializer.Deserialize<List<Product>>(json, Options) ?? new List<Product>();
        }
        catch (JsonException) { return new List<Product>(); }
    }

    public void Save(List<Product> products)
    {
        var json = JsonSerializer.Serialize(products, Options);
        File.WriteAllText(_path, json);
    }
}