namespace LostAndFoundPlatform.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Category? Category { get; set; }
    public int CategoryId { get; set; }
    public string ImageUrl { get; set; }
    public Report? Report { get; set; }
    
}