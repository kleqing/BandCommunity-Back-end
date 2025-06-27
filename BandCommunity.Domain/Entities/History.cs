using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class History
{
    public Guid HistoryId { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public EntityEnum.EntitiesType ContentType { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; }
}