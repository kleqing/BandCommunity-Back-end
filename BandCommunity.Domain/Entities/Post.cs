using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Post
{
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public EntityEnum.EntitiesType AuthorType { get; set; }
    public string Content { get; set; }
    public EntityEnum.VisibilityStatus Visibility { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }    
    public bool RequiredApproval { get; set; }
    
    public virtual User User { get; set; }
    public virtual ICollection<Comment> Comment { get; set; }
    public virtual ICollection<PostReaction> PostReactions { get; set; }
}