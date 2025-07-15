using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Report
{
    public Guid ReportId { get; set; }
    public Guid ReporterId { get; set; }
    public Guid ContentId { get; set; }
    public EntityEnum.EntitiesType EntityType { get; set; }
    public string? Reason { get; set; }
    public EntityEnum.Status? Status { get; set; }
    public EntityEnum.SystemRole ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; } = null!;
}