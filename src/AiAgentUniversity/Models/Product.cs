namespace AiAgentUniversity.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }

    public int DaysLeft() => (ExpiryDate.Date - DateTime.Today).Days;
}