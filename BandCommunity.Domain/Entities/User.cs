using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BandCommunity.Domain.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    [MaxLength(20)] 
    public override string? UserName { get; set; }
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    [MaxLength(11)]
    [MinLength(10)]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "The phone number must contain only digits and be 10–11 digits long.")]
    public override string? PhoneNumber { get; set; }

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
    
    public virtual Band Band { get; set; }
    public virtual ICollection<Follow> Follow { get; set; }
    public virtual ICollection<Share> Share { get; set; }
    public virtual ICollection<Like> Like { get; set; }
    public virtual ICollection<History> History { get; set; }
    public virtual ICollection<Notification> Notification { get; set; }
    public virtual ICollection<Report> Report { get; set; }
    public virtual ICollection<Post> Post { get; set; }
    public virtual ICollection<UserRestriction> UserRestriction { get; set; }
    public virtual ICollection<Messages> Messages { get; set; }
    public virtual ICollection<ConversationMember> ConversationMember { get; set; }
    public virtual ICollection<GroupMember> GroupMember { get; set; }
    public virtual ICollection<Groups> Groups { get; set; }
    public virtual ICollection<BandMember> BandMembers { get; set; }
    public virtual ICollection<Music> Music { get; set; }
    public virtual ICollection<Playlist> Playlist { get; set; }
    public virtual ICollection<Appeal> Appeals { get; set; }
}