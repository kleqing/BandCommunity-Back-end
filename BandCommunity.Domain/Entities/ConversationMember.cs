namespace BandCommunity.Domain.Entities;

public class ConversationMember
{
    public Guid ConversationMemberId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}