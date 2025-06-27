namespace BandCommunity.Domain.Entities;

public class Comment
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; } //* For threaded comments
    public string? Content { get; set; }
    public string? MediaUrl { get; set; } //* Optional media attachment
    public DateTime CommentAt { get; set; } = DateTime.UtcNow;
    
    public virtual Post Post { get; set; }
    public virtual ICollection<CommentReaction> CommentReaction { get; set; }
}