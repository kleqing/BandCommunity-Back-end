using System.ComponentModel.DataAnnotations;
using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Groups
{
    public Guid GroupId { get; set; }
    public Guid CreatorId { get; set; }
    
    [Required]
    public string GroupName { get; set; }
    public string? GroupDescription { get; set; }
    public EntityEnum.VisibilityStatus Visibility { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public virtual ICollection<GroupMember> GroupMembers { get; set; }
    public virtual User User { get; set; }
}