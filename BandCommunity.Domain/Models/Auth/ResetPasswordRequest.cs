using System.ComponentModel.DataAnnotations;

namespace BandCommunity.Domain.Models.Auth;

public class ResetPasswordRequest
{
    public string? Token { get; set; }
    [Required]
    public string NewPassword { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}