using System.ComponentModel.DataAnnotations;
using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Music
{
    public Guid MusicId { get; set; }
    public Guid AlbumId { get; set; }
    public Guid UploaderId { get; set; }
    public Guid BandId { get; set; }
    [Required]
    public EntityEnum.VisibilityStatus Visibility { get; set; }
    
    [Required]
    public string Title { get; set; } = null!;
    
    [Required]
    public string FileUrl { get; set; } = null!;
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<AlbumMusic> AlbumMusics { get; set; } = null!;
    public virtual ICollection<PlaylistMusic> PlaylistMusics { get; set; } = null!;
    public virtual Band Band { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}