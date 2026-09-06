using System.ComponentModel.DataAnnotations;

namespace LostAndFoundPlatform.Models;

public class Message
{
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    public DateTime SentAt { get; set; }

    [Required]
    public string SenderId { get; set; }
    public ApplicationUser? Sender { get; set; }

    [Required]
    public string ReceiverId { get; set; }
    public ApplicationUser? Receiver { get; set; }

    public int ReportId { get; set; }
    public Report? Report { get; set; }
}