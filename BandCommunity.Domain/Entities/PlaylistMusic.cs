namespace BandCommunity.Domain.Entities;

public class PlaylistMusic
{
    public Guid PlaylistMusicId { get; set; }
    public Guid PlaylistId { get; set; }
    public Guid MusicId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Playlist Playlist { get; set; } = null!;
    public virtual Music Music { get; set; } = null!;
}