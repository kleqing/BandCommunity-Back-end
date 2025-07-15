using System.ComponentModel.DataAnnotations;
using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Band
{
    public Guid BandId { get; set; }
    public Guid CreatorId { get; set; }
    [Required]
    public string BandName { get; set; } = null!;
    public Genre BandGenre { get; set; }
    [MaxLength(150)]
    public string? BandDescription { get; set; }
    public EntityEnum.VisibilityStatus Visibility { get; set; }
    public string? BandProfilePictureUrl { get; set; } = null!;
    public string? BandCoverPictureUrl { get; set; } = null!;
    public bool IsDissolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DissolvedAt { get; set; }
    
    public virtual User User { get; set; } = null!;
    public virtual ICollection<BandMember> BandMembers { get; set; } = null!;
    public virtual ICollection<Album> Albums { get; set; } = null!;
    public virtual ICollection<Music> Musics { get; set; } = null!;
}