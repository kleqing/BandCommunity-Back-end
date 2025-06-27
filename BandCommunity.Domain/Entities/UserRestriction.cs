using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class UserRestriction
{
    public Guid RestrictionId { get; set; }
    public Guid UserId { get; set; }
    public Guid ImposedBy { get; set; } //* This is the user who imposed the restriction, could be an admin or moderator
    public EntityEnum.RestrictionLevel RestrictionLevel { get; set; }
    public EntityEnum.RestrictionFeature RestrictionFeature { get; set; }
    public string? Reason { get; set; }
    public DateTime ExpireAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual User User { get; set; }
    public virtual ICollection<Appeal> Appeal { get; set; }
}