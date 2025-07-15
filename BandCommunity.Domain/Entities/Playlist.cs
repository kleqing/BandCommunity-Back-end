using System.ComponentModel.DataAnnotations;
using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Playlist
{
    public Guid PlaylistId { get; set; }
    public Guid CreatorId { get; set; }
    
    [Required]
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public EntityEnum.VisibilityStatus Visibility { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<PlaylistMusic> PlaylistMusics { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}