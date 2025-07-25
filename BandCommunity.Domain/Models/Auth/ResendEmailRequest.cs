﻿using System.ComponentModel.DataAnnotations;

namespace BandCommunity.Domain.Models.Auth;

public class ResendEmailRequest
{
    [Required]
    public string Email { get; set; } = null!;
}