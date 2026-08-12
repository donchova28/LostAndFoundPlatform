namespace LostAndFoundPlatform.Models;
using Microsoft.AspNetCore.Identity;


public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}