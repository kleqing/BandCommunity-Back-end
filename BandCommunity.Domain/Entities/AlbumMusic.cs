namespace BandCommunity.Domain.Entities;

public class AlbumMusic
{
    public Guid AlbumMusicId { get; set; }
    public Guid AlbumId { get; set; }
    public Guid MusicId { get; set; }

    public virtual Album Album { get; set; } = null!;
    public virtual Music Music { get; set; } = null!;
}