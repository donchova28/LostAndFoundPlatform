namespace LostAndFoundPlatform.Models;

public class Location
{
    public int Id { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    
    public string? ApplicationUserId { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }

}