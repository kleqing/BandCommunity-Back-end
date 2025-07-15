using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Like
{
    public Guid LikeId { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public EntityEnum.EntitiesType ContentType { get; set; }
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; } = null!;
}