using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class GroupMember
{
    public Guid GroupMemberId { get; set; }
    public Guid GroupId { get; set; }
    public Guid AuthorId { get; set; }
    public EntityEnum.GroupRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; }
    public virtual Groups Group { get; set; }
}