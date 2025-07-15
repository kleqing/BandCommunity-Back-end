using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class CommentReaction
{
    public Guid ReactionId { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public EntityEnum.ReactionType ReactionType { get; set; }
    public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Comment Comment { get; set; } = null!;
}