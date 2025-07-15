using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Share
{
    public Guid ShareId { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public EntityEnum.EntitiesType ContentType { get; set; }
    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; } = null!;
}