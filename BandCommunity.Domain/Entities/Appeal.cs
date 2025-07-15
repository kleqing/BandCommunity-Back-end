using System.ComponentModel.DataAnnotations;
using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Appeal
{
    public Guid AppealId { get; set; }
    public Guid RestrictionId { get; set; }
    public Guid UserId { get; set; }
    
    [Required]
    public string? Content { get; set; }
    public EntityEnum.Status Status { get; set; }
    public EntityEnum.SystemRole ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual UserRestriction UserRestriction { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}