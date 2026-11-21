using AiAgentUniversity.Domain.Models;

namespace AiAgentUniversity.Domain.Interfaces;

public interface IStorage
{
    List<Product> Load();
    void Save(List<Product> products);
}
