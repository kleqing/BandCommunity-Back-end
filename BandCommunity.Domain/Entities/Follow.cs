using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Follow
{
    public Guid FollowId { get; set; }
    public Guid FollowerId { get; set; }
    public Guid FollowingEntityId { get; set; }
    public EntityEnum.EntitiesType FollowingEntityType { get; set; } //* "User", "Band", etc.
    public DateTime FollowAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; }
}