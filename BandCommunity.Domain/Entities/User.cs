using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BandCommunity.Domain.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    public string FirstName { get; set; } = null!;

    [Required] public string LastName { get; set; } = null!;
    
    [Required]
    [MaxLength(20)] 
    public override string? UserName { get; set; }
    public DateTime DateOfBirth { get; set; }
    
    [MaxLength(11)]
    [MinLength(10)]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "The phone number must contain only digits and be 10–11 digits long.")]
    public override string? PhoneNumber { get; set; }
    
    public string? Address { get; set; }

    [MaxLength(150)]
    public string? Bio { get; set; }
    public string? RestrictionLevel { get; set; }
    public DateTime? RestrictionEndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    //* JWT
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public virtual Band Band { get; set; } = null!;
    public virtual ICollection<Follow> Follow { get; set; } = null!;
    public virtual ICollection<Share> Share { get; set; } = null!;
    public virtual ICollection<Like> Like { get; set; } = null!;
    public virtual ICollection<History> History { get; set; } = null!;
    public virtual ICollection<Notification> Notification { get; set; } = null!;
    public virtual ICollection<Report> Report { get; set; } = null!;
    public virtual ICollection<Post> Post { get; set; } = null!;
    public virtual ICollection<UserRestriction> UserRestriction { get; set; } = null!;
    public virtual ICollection<Messages> Messages { get; set; } = null!;
    public virtual ICollection<ConversationMember> ConversationMember { get; set; } = null!;
    public virtual ICollection<GroupMember> GroupMember { get; set; } = null!;
    public virtual ICollection<Groups> Groups { get; set; } = null!;
    public virtual ICollection<BandMember> BandMembers { get; set; } = null!;
    public virtual ICollection<Music> Music { get; set; } = null!;
    public virtual ICollection<Playlist> Playlist { get; set; } = null!;
    public virtual ICollection<Appeal> Appeals { get; set; } = null!;
}