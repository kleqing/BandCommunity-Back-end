using BandCommunity.Domain.Enums;

namespace BandCommunity.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; } //* User or Band receiving the notification
    public Guid ActorId { get; set; } //* User or Band that triggered the notification
    public Guid ContentId { get; set; } //* ID of the content that triggered the notification (e.g., Post, Comment, etc.)
    public EntityEnum.NotificationType ActionType { get; set; } //* Type of action that triggered the notification
    public EntityEnum.EntitiesType EntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTime NotifyAt { get; set; }
    
    public virtual User User { get; set; }
}