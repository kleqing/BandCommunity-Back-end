using BandCommunity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BandCommunity.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Album> Albums { get; set; }
    public DbSet<AlbumMusic> AlbumMusics { get; set; }
    public DbSet<Appeal> Appeals { get; set; }
    public DbSet<Band> Bands { get; set; }
    public DbSet<BandMember> BandMembers { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentReaction> CommentReactions { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMember> ConversationMembers { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<Groups> Groups { get; set; }
    public DbSet<History> Histories { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Messages> Messages { get; set; }
    public DbSet<Music> Musics { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistMusic> PlaylistMusics { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostReaction> PostReactions { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Share> Shares { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRestriction> UserRestrictions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        //* Configure the primary key
        
        builder.Entity<Album>()
            .HasKey(a => a.AlbumId);

        builder.Entity<AlbumMusic>()
            .HasKey(a => a.AlbumMusicId);

        builder.Entity<Appeal>()
            .HasKey(a => a.AppealId);
        
        builder.Entity<Band>()
            .HasKey(b => b.BandId);
        
        builder.Entity<BandMember>()
            .HasKey(b => b.BandMemberId);
        
        builder.Entity<Comment>()
            .HasKey(c => c.CommentId);
        
        builder.Entity<CommentReaction>()
            .HasKey(c => c.ReactionId);
        
        builder.Entity<Conversation>()
            .HasKey(c => c.ConversationId);
        
        builder.Entity<ConversationMember>()
            .HasKey(c => c.ConversationMemberId);
        
        builder.Entity<Follow>()
            .HasKey(f => f.FollowId);
        
        builder.Entity<GroupMember>()
            .HasKey(g => g.GroupMemberId);
        
        builder.Entity<Groups>()
            .HasKey(g => g.GroupId);
        
        builder.Entity<History>()
            .HasKey(h => h.HistoryId);
        
        builder.Entity<Like>()
            .HasKey(l => l.LikeId);
        
        builder.Entity<Messages>()
            .HasKey(m => m.MessageId);
        
        builder.Entity<Music>()
            .HasKey(m => m.MusicId);
        
        builder.Entity<Notification>()
            .HasKey(n => n.NotificationId);
        
        builder.Entity<Playlist>()
            .HasKey(p => p.PlaylistId);
        
        builder.Entity<PlaylistMusic>()
            .HasKey(p => p.PlaylistMusicId);
        
        builder.Entity<Post>()
            .HasKey(p => p.PostId);
        
        builder.Entity<PostReaction>()
            .HasKey(p => p.PostReactionId);
        
        builder.Entity<Report>()
            .HasKey(r => r.ReportId);
        
        builder.Entity<Share>()
            .HasKey(s => s.ShareId);
        
        builder.Entity<UserRestriction>()
            .HasKey(u => u.RestrictionId);
        
        //* Unique constraints
        
        builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
        
        //* Fix timestamp in Postgres
        builder.Entity<Album>().Property(a => a.ReleaseDate).HasColumnType("timestamp with time zone");
        builder.Entity<Appeal>().Property(a => a.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Band>().Property(b => b.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Band>().Property(b => b.DissolvedAt).HasColumnType("timestamp with time zone");
        builder.Entity<BandMember>().Property(b => b.JoinedAt).HasColumnType("timestamp with time zone");
        builder.Entity<BandMember>().Property(b => b.LeftAt).HasColumnType("timestamp with time zone");
        builder.Entity<Comment>().Property(c => c.CommentAt).HasColumnType("timestamp with time zone");
        builder.Entity<CommentReaction>().Property(c => c.ReactedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Conversation>().Property(c => c.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<ConversationMember>().Property(c => c.JoinedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Follow>().Property(f => f.FollowAt).HasColumnType("timestamp with time zone");
        builder.Entity<GroupMember>().Property(g => g.JoinedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Groups>().Property(g => g.CreatedDate).HasColumnType("timestamp with time zone");
        builder.Entity<History>().Property(h => h.ViewedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Like>().Property(l => l.LikedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Messages>().Property(m => m.SendAt).HasColumnType("timestamp with time zone");
        builder.Entity<Music>().Property(m => m.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Notification>().Property(n => n.NotifyAt).HasColumnType("timestamp with time zone");
        builder.Entity<Playlist>().Property(p => p.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<PlaylistMusic>().Property(p => p.AddedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Post>().Property(p => p.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<PostReaction>().Property(p => p.ReactedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Report>().Property(r => r.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<Share>().Property(s => s.SharedAt).HasColumnType("timestamp with time zone");
        builder.Entity<User>().Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<User>().Property(u => u.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Entity<User>().Property(u => u.DateOfBirth).HasColumnType("timestamp with time zone");
        builder.Entity<User>().Property(u => u.RestrictionEndDate).HasColumnType("timestamp with time zone");
        builder.Entity<User>().Property(u => u.RefreshTokenExpiryTime).HasColumnType("timestamp with time zone");
        builder.Entity<UserRestriction>().Property(u => u.ExpireAt).HasColumnType("timestamp with time zone");
        builder.Entity<UserRestriction>().Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
        
        //* Configure relationships
        builder.Entity<User>()
            .HasMany(f => f.Follow)
            .WithOne(u => u.User)
            .HasForeignKey(f => f.FollowerId);

        builder.Entity<User>()
            .HasMany(s => s.Share)
            .WithOne(u => u.User)
            .HasForeignKey(s => s.UserId);
        
        builder.Entity<User>()
            .HasMany(l => l.Like)
            .WithOne(u => u.User)
            .HasForeignKey(l => l.UserId);
        
        builder.Entity<User>()
            .HasMany(h => h.History)
            .WithOne(u => u.User)
            .HasForeignKey(c => c.UserId);
        
        builder.Entity<User>()
            .HasMany(r => r.Report)
            .WithOne(u => u.User)
            .HasForeignKey(r => r.ReporterId);

        builder.Entity<User>()
            .HasMany(n => n.Notification)
            .WithOne(u => u.User)
            .HasForeignKey(n => n.RecipientId);
        
        builder.Entity<User>()
            .HasMany(u => u.UserRestriction)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);
        
        builder.Entity<User>()
            .HasMany(a => a.Appeals)
            .WithOne(u => u.User)
            .HasForeignKey(a => a.UserId);

        builder.Entity<User>()
            .HasMany(p => p.Post)
            .WithOne(u => u.User)
            .HasForeignKey(p => p.AuthorId);

        builder.Entity<User>()
            .HasMany(g => g.GroupMember)
            .WithOne(u => u.User)
            .HasForeignKey(g => g.AuthorId);
        
        builder.Entity<User>()
            .HasMany(g => g.Groups)
            .WithOne(u => u.User)
            .HasForeignKey(g => g.CreatorId);
        
        builder.Entity<User>()
            .HasMany(b => b.BandMembers)
            .WithOne(u => u.User)
            .HasForeignKey(b => b.UserId);
        
        builder.Entity<User>()
            .HasOne(b => b.Band)
            .WithOne(u => u.User)
            .HasForeignKey<Band>(b => b.CreatorId);
        
        builder.Entity<User>()
            .HasMany(m => m.Music)
            .WithOne(u => u.User)
            .HasForeignKey(m => m.UploaderId);
        
        builder.Entity<User>()
            .HasMany(p => p.Playlist)
            .WithOne(u => u.User)
            .HasForeignKey(p => p.CreatorId);
        
        builder.Entity<User>()
            .HasMany(m => m.Messages)
            .WithOne(u => u.User)
            .HasForeignKey(m => m.SenderId);
        
        builder.Entity<User>()
            .HasMany(c => c.ConversationMember)
            .WithOne(u => u.User)
            .HasForeignKey(c => c.UserId);
        
        builder.Entity<Conversation>()
            .HasMany(c => c.ConversationMember)
            .WithOne(c => c.Conversation)
            .HasForeignKey(c => c.ConversationId);
        
        builder.Entity<Conversation>()
            .HasMany(m => m.Messages)
            .WithOne(c => c.Conversation)
            .HasForeignKey(m => m.ConversationId);
        
        builder.Entity<UserRestriction>()
            .HasMany(a => a.Appeal)
            .WithOne(u => u.UserRestriction)
            .HasForeignKey(a => a.RestrictionId);
        
        builder.Entity<GroupMember>()
            .HasOne(u => u.User)
            .WithMany(g => g.GroupMember)
            .HasForeignKey(g => g.AuthorId);
        
        builder.Entity<GroupMember>()
            .HasOne(g => g.Group)
            .WithMany(g => g.GroupMembers)
            .HasForeignKey(g => g.GroupId);
        
        builder.Entity<BandMember>()
            .HasOne(u => u.User)
            .WithMany(b => b.BandMembers)
            .HasForeignKey(b => b.UserId);
        
        builder.Entity<BandMember>()
            .HasOne(b => b.Band)
            .WithMany(b => b.BandMembers)
            .HasForeignKey(b => b.BandId);
        
        builder.Entity<Band>()
            .HasMany(b => b.Albums)
            .WithOne(a => a.Band)
            .HasForeignKey(b => b.BandId);

        builder.Entity<Band>()
            .HasMany(b => b.Musics)
            .WithOne(m => m.Band)
            .HasForeignKey(m => m.BandId);

        builder.Entity<AlbumMusic>()
            .HasOne(a => a.Album)
            .WithMany(am => am.AlbumMusics)
            .HasForeignKey(a => a.AlbumId);
        
        builder.Entity<AlbumMusic>()
            .HasOne(a => a.Music)
            .WithMany(m => m.AlbumMusics)
            .HasForeignKey(a => a.MusicId);
        
        builder.Entity<PlaylistMusic>()
            .HasOne(p => p.Playlist)
            .WithMany(pm => pm.PlaylistMusics)
            .HasForeignKey(p => p.PlaylistId);
        
        builder.Entity<PlaylistMusic>()
            .HasOne(p => p.Music)
            .WithMany(m => m.PlaylistMusics)
            .HasForeignKey(p => p.MusicId);
    }
}