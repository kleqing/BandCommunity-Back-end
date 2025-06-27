using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class PostReaction
{
    public Guid PostReactionId { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public EntityEnum.ReactionType ReactionType { get; set; }
    public DateTime ReactedAt { get; set; }
    public virtual Post Post { get; set; }
}