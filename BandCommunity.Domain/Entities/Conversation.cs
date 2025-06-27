using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Conversation
{
    public Guid ConversationId { get; set; }
    public Guid? BandId { get; set; }
    public Guid? GroupId { get; set; }
    public EntityEnum.VisibilityStatus GroupType { get; set; } //* Public, Private
    public string? GroupName { get; set; }
    public string? GroupAvatarImage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Messages> Messages { get; set; }
    public virtual ICollection<ConversationMember> ConversationMember { get; set; }
}