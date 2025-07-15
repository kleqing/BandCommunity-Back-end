using System.ComponentModel.DataAnnotations;

namespace BandCommunity.Domain.Models.Auth;

public class ForgotPasswordRequest
{
    [Required]
    public string Email { get; set; } = null!;
}