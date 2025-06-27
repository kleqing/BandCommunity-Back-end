using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class BandMember
{
    public Guid BandMemberId { get; set; }
    public Guid BandId { get; set; }
    public Guid UserId { get; set; }
    public EntityEnum.BandRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    
    public virtual User User { get; set; }
    public virtual Band Band { get; set; }
}