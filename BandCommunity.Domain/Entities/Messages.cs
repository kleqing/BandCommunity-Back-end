namespace BandCommunity.Domain.Entities;

public class Messages
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string? EncryptedContent { get; set; }
    public DateTime SendAt { get; set; }
    
    public virtual Conversation Conversation { get; set; }
    public virtual User User { get; set; }
}