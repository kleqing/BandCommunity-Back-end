namespace BandCommunity.Domain.Enums;

public class EntityEnum
{
    public enum EntitiesType
    {
        User = 0,
        Band = 1,
        Group = 2,
        Post = 3,
        Comment = 4,
        Track = 5,
        Album = 6,
        Playlist = 7,
        Event = 8
    }

    public enum SystemRole
    {
        Administrator = 0,
        Moderator = 1,
        User = 2,
        Guest = 3
    }
    
    public enum BandRole
    {
        Leader = 0,
        CoLeader = 1,
        Member = 2,
        FormerMember = 3
    }
    
    public enum GroupRole
    {
        GroupAdmin = 0,
        GroupModerator = 1,
        GroupMember = 2
    }
    
    public enum VisibilityStatus
    {
        Public = 0,
        Private = 1,
        Hidden = 2
    }

    public enum AccountStatus
    {
        Active = 0,
        Restricted = 1,
        Banned = 2,
        PendingVerification = 3
    }

    public enum Status
    {
        Pending = 0,
        InProgress = 1,
        Resolved = 2,
        Rejected = 3
    }
    
    public enum RestrictionLevel
    {
        None = 0,
        Warning = 1,
        Temporary = 2,
        Permanent = 3
    }

    public enum NotificationType
    {
        Like = 0,
        Comment = 1,
        Follow = 2,
        Share = 3,
        BandInvite = 4,
        SystemAnnouncement = 5,
        ReportUpdate = 6
    }
    
    public enum ReactionType
    {
        Like = 0,
        Dislike = 1,
        Love = 2,
        Angry = 3,
        Sad = 4,
        Haha = 5
    }

    public enum RestrictionFeature
    {
        Posting = 0,
        Uploading = 1,
        Commenting = 2,
        Chatting = 3,
        All = 4
    }
}