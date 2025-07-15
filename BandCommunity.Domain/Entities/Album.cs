using System.ComponentModel.DataAnnotations;
using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Album
{
    public Guid AlbumId { get; set; }
    public Guid BandId { get; set; }
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    public EntityEnum.VisibilityStatus Visibility { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? CoverArtUrl { get; set; }
    
    public virtual Band Band { get; set; } = null!;
    public virtual ICollection<AlbumMusic> AlbumMusics { get; set; } = null!;
}