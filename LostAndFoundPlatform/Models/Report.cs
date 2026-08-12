namespace LostAndFoundPlatform.Models;

public class Report
{
    public int Id { get; set; }
    public int Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public ApplicationUser? ApplicationUser { get; set; }
    public string ApplicationUserId { get; set; }
    
    public Item? Item  { get; set; }
    public int ItemId { get; set; }
    
    public int? EventLocationId { get; set; }
    public Location? EventLocation { get; set; }

    public int? PickupLocationId { get; set; }
    public Location? PickupLocation { get; set; }
}