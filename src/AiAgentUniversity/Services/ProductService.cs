using AiAgentUniversity.Models;
using AiAgentUniversity.Storage;

namespace AiAgentUniversity.Services;

public class ProductService
{
    private readonly JsonStorage _storage;
    private readonly List<Product> _products;
    private int _nextId;

    // Конструктор с 1 аргументом (JsonStorage)
    public ProductService(JsonStorage storage)
    {
        _storage = storage;
        _products = _storage.Load();
        _nextId = _products.Count == 0 ? 1 : _products.Max(p => p.Id) + 1;
    }

    public IReadOnlyList<Product> All() => _products;

    public Product Add(string name, double quantity, string unit, string category, DateTime expiry)
    {
        var product = new Product { Id = _nextId++, Name = name, Quantity = quantity, Unit = unit, Category = category, ExpiryDate = expiry };
        _products.Add(product);
        _storage.Save(_products);
        return product;
    }

    public IReadOnlyList<Product> ExpiringWithin(int days) => _products.Where(p => p.DaysLeft() >= 0 && p.DaysLeft() <= days).OrderBy(p => p.ExpiryDate).ToList();
    public IReadOnlyList<Product> ByCategory(string category) => _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).OrderBy(p => p.ExpiryDate).ToList();
    public IReadOnlyList<Product> Search(string query) => _products.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    public Product? FindById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public bool Consume(int id, double amount, out string message)
    {
        var product = FindById(id);
        if (product is null) { message = $"Продукт с id={id} не найден."; return false; }
        if (amount <= 0) { message = "Количество должно быть положительным."; return false; }
        if (amount > product.Quantity) { message = $"Нельзя списать {amount}, осталось {product.Quantity} {product.Unit}."; return false; }

        product.Quantity -= amount;
        if (product.Quantity <= 0.0001)
        {
            _products.Remove(product);
            message = $"Продукт #{product.Id} «{product.Name}» полностью использован.";
        }
        else
        {
            message = $"Списано {amount} {product.Unit} «{product.Name}». Осталось: {product.Quantity} {product.Unit}.";
        }
        _storage.Save(_products);
        return true;
    }
}