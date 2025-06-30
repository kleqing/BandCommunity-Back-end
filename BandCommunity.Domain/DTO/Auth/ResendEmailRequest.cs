using System.ComponentModel.DataAnnotations;

namespace BandCommunity.Domain.DTO.Auth;

public class ResendEmailRequest
{
    [Required]
    public string Email { get; set; }
}