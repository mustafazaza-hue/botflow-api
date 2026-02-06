using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BotFlow.Domain.Entities;
using BotFlow.Domain.Enums;

namespace BotFlow.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // إضافة جميع DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<SocialPage> SocialPages { get; set; }
        public DbSet<Bot> Bots { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<RecentActivity> RecentActivities { get; set; }
        
        // إضافة الـ DbSets الجديدة
        public DbSet<AnalyticsData> AnalyticsData { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<ConversationTag> ConversationTags { get; set; }

        // ========== إضافة DbSets للسوبر أدمن ==========
        public DbSet<AIDataSource> AIDataSources { get; set; }
        public DbSet<KPIMetric> KPIMetrics { get; set; }
        public DbSet<SystemStatistic> SystemStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User configuration
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
                entity.HasIndex(u => u.UserName).IsUnique();
                
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.CompanyName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(u => u.PasswordSalt).IsRequired().HasMaxLength(500);
                
                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                
                entity.Property(u => u.SubscriptionPlan).HasMaxLength(50);
                entity.Property(u => u.ProfileImageUrl).HasMaxLength(500);
                
                entity.Property(u => u.EmailVerificationToken).HasMaxLength(500);
                entity.Property(u => u.PasswordResetToken).HasMaxLength(500);
                entity.Property(u => u.RefreshToken).HasMaxLength(500);
                entity.Property(u => u.GoogleId).HasMaxLength(100);
                entity.Property(u => u.FacebookId).HasMaxLength(100);
                entity.Property(u => u.MicrosoftId).HasMaxLength(100);
                
                entity.HasQueryFilter(u => u.IsActive);
            });

            // SocialPage configuration
            builder.Entity<SocialPage>(entity =>
            {
                entity.Property(p => p.PageName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.PageId).IsRequired().HasMaxLength(100);
                entity.Property(p => p.AccessToken).IsRequired().HasMaxLength(500);
                
                // تغيير Platform لاستخدام Enum
                entity.Property(p => p.Platform)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            // Bot configuration (القديم)
            builder.Entity<Bot>(entity =>
            {
                entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.Description).HasMaxLength(500);
                entity.Property(b => b.Color).HasMaxLength(50);
                entity.Property(b => b.Type).HasMaxLength(50);
                
                // تغيير Status لاستخدام Enum
                entity.Property(b => b.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            // Conversation configuration (القديم)
            builder.Entity<Conversation>(entity =>
            {
                entity.Property(c => c.CustomerName).HasMaxLength(100);
                entity.Property(c => c.CustomerId).HasMaxLength(100);
                entity.Property(c => c.Message).IsRequired().HasMaxLength(2000);
                entity.Property(c => c.BotResponse).HasMaxLength(2000);
                
                // تغيير Platform لاستخدام Enum
                entity.Property(c => c.Platform)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                
                // تغيير Status لاستخدام Enum
                entity.Property(c => c.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            // RecentActivity configuration
            builder.Entity<RecentActivity>(entity =>
            {
                entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Description).IsRequired().HasMaxLength(500);
                entity.Property(a => a.ActivityType).HasMaxLength(50);
                entity.Property(a => a.Icon).HasMaxLength(50);
                entity.Property(a => a.Color).HasMaxLength(50);
            });

            // ===== إضافة تكوينات الـ Models الجديدة =====

            // AnalyticsData configuration
            builder.Entity<AnalyticsData>(entity =>
            {
                entity.Property(a => a.MetricType).IsRequired().HasMaxLength(50);
                entity.Property(a => a.MetricName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Platform).HasMaxLength(50);
                entity.Property(a => a.Period).HasMaxLength(20);
                
                entity.HasIndex(a => a.Timestamp);
                entity.HasIndex(a => new { a.MetricType, a.Date });
                
                entity.HasOne(a => a.Bot)
                    .WithMany(b => b.Analytics)
                    .HasForeignKey(a => a.BotId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(a => a.Page)
                    .WithMany(p => p.Analytics)
                    .HasForeignKey(a => a.PageId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Message configuration
            builder.Entity<Message>(entity =>
            {
                entity.Property(m => m.Content).IsRequired();
                entity.Property(m => m.SenderType).IsRequired().HasMaxLength(20);
                entity.Property(m => m.MessageType).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(m => m.CreatedAt);
                entity.HasIndex(m => m.ConversationId);
                
                entity.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(m => m.SentBy)
                    .WithMany()
                    .HasForeignKey(m => m.SentByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Page configuration (لـ Pages Integration)
            builder.Entity<Page>(entity =>
            {
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Platform).IsRequired().HasMaxLength(50);
                entity.Property(p => p.PlatformId).IsRequired().HasMaxLength(200);
                entity.Property(p => p.AccessToken).IsRequired().HasMaxLength(500);
                entity.Property(p => p.ProfilePictureUrl).HasMaxLength(500);
                entity.Property(p => p.ConnectionStatus).HasMaxLength(50);
                entity.Property(p => p.PermissionsStatus).HasMaxLength(50);
                entity.Property(p => p.WebhookUrl).HasMaxLength(500);
                entity.Property(p => p.WebhookSecret).HasMaxLength(500);
                
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.PlatformId);
                
                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(p => p.Bot)
                    .WithMany(b => b.Pages)
                    .HasForeignKey(p => p.BotId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TeamMember configuration
            builder.Entity<TeamMember>(entity =>
            {
                entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Email).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Role).IsRequired().HasMaxLength(50);
                entity.Property(t => t.AvatarUrl).HasMaxLength(500);
                entity.Property(t => t.Status).HasMaxLength(20);
                entity.Property(t => t.InvitationToken).HasMaxLength(500);
                
                entity.HasIndex(t => t.Email);
                entity.HasIndex(t => t.WorkspaceId);
                entity.HasIndex(t => t.UserId);
                
                entity.HasOne(t => t.Workspace)
                    .WithMany(w => w.TeamMembers)
                    .HasForeignKey(t => t.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Workspace configuration
            builder.Entity<Workspace>(entity =>
            {
                entity.Property(w => w.Name).IsRequired().HasMaxLength(200);
                entity.Property(w => w.LogoUrl).HasMaxLength(500);
                entity.Property(w => w.BrandColor).HasMaxLength(50);
                entity.Property(w => w.Domain).HasMaxLength(100);
                entity.Property(w => w.Timezone).HasMaxLength(50);
                entity.Property(w => w.Language).HasMaxLength(10);
                entity.Property(w => w.Plan).HasMaxLength(50);
                
                entity.HasIndex(w => w.OwnerId);
                
                entity.HasOne(w => w.Owner)
                    .WithMany()
                    .HasForeignKey(w => w.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ConversationTag configuration
            builder.Entity<ConversationTag>(entity =>
            {
                entity.Property(ct => ct.Tag).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(ct => new { ct.ConversationId, ct.Tag }).IsUnique();
                
                entity.HasOne(ct => ct.Conversation)
                    .WithMany(c => c.Tags)
                    .HasForeignKey(ct => ct.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(ct => ct.AddedBy)
                    .WithMany()
                    .HasForeignKey(ct => ct.AddedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Bot configuration (الجديد)
            builder.Entity<Domain.Entities.Bot>(entity =>
            {
                entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
                entity.Property(b => b.Description).HasMaxLength(1000);
                entity.Property(b => b.Status).IsRequired().HasMaxLength(50);
                entity.Property(b => b.FlowConfiguration).IsRequired();
                entity.Property(b => b.WelcomeMessage).HasMaxLength(500);
                entity.Property(b => b.FallbackMessage).HasMaxLength(500);
                
                entity.HasIndex(b => b.UserId);
                entity.HasIndex(b => b.Status);
                
                entity.HasOne(b => b.User)
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasMany(b => b.Pages)
                    .WithOne(p => p.Bot)
                    .HasForeignKey(p => p.BotId);
                    
                entity.HasMany(b => b.Conversations)
                    .WithOne(c => c.Bot)
                    .HasForeignKey(c => c.BotId);
                    
                entity.HasMany(b => b.Analytics)
                    .WithOne(a => a.Bot)
                    .HasForeignKey(a => a.BotId);
            });

            // Conversation configuration (الجديد)
            builder.Entity<Domain.Entities.Conversation>(entity =>
            {
                entity.Property(c => c.UserName).IsRequired().HasMaxLength(200);
                entity.Property(c => c.PlatformId).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Platform).IsRequired().HasMaxLength(50);
                entity.Property(c => c.AvatarUrl).HasMaxLength(500);
                entity.Property(c => c.Status).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Priority).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(c => c.PlatformId);
                entity.HasIndex(c => c.BotId);
                entity.HasIndex(c => c.PageId);
                entity.HasIndex(c => c.Status);
                entity.HasIndex(c => c.LastMessageAt);
                
                entity.HasOne(c => c.Bot)
                    .WithMany(b => b.Conversations)
                    .HasForeignKey(c => c.BotId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(c => c.Page)
                    .WithMany(p => p.Conversations)
                    .HasForeignKey(c => c.PageId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(c => c.AssignedTo)
                    .WithMany()
                    .HasForeignKey(c => c.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasMany(c => c.Messages)
                    .WithOne(m => m.Conversation)
                    .HasForeignKey(m => m.ConversationId);
                    
                entity.HasMany(c => c.Tags)
                    .WithOne(t => t.Conversation)
                    .HasForeignKey(t => t.ConversationId);
            });

            // ========== إضافة تكوينات Models للسوبر أدمن ==========

            // AIDataSource configuration
            builder.Entity<AIDataSource>(entity =>
            {
                entity.ToTable("AIDataSources");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasConversion(
                        v => v.ToString(),
                        v => v);
                    
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasConversion(
                        v => v.ToString(),
                        v => v);
                    
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);
                    
                entity.Property(e => e.FileType)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.FileUrl)
                    .HasMaxLength(500);
                    
                entity.Property(e => e.Url)
                    .HasMaxLength(500);
                    
                entity.Property(e => e.ApiEndpoint)
                    .HasMaxLength(500);
                    
                entity.Property(e => e.DatabaseType)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(1000);
                    
                entity.Property(e => e.ProgressPercentage)
                    .HasPrecision(5, 2);
                    
                entity.Property(e => e.FileSize)
                    .HasColumnType("bigint");
                    
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                    
                entity.Property(e => e.UpdatedAt)
                    .IsRequired();
                    
                entity.Property(e => e.LastProcessedAt);
                    
                // العلاقات
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                // الفهارس
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.Type, e.Status });
                entity.HasIndex(e => new { e.UserId, e.Status });
            });

            // KPIMetric configuration
            builder.Entity<KPIMetric>(entity =>
            {
                entity.ToTable("KPIMetrics");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.MetricType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasConversion(
                        v => v.ToString(),
                        v => v);
                    
                entity.Property(e => e.Period)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasConversion(
                        v => v.ToString(),
                        v => v);
                    
                entity.Property(e => e.Date)
                    .IsRequired();
                    
                entity.Property(e => e.Value)
                    .HasPrecision(18, 4);
                    
                entity.Property(e => e.TargetValue)
                    .HasPrecision(18, 4);
                    
                entity.Property(e => e.ChangePercentage)
                    .HasPrecision(5, 2);
                    
                entity.Property(e => e.Platform)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                    
                entity.Property(e => e.UpdatedAt)
                    .IsRequired();
                    
                // الفهارس
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.MetricType);
                entity.HasIndex(e => e.Period);
                entity.HasIndex(e => e.Platform);
                entity.HasIndex(e => new { e.MetricType, e.Date });
                entity.HasIndex(e => new { e.MetricType, e.Period, e.Date });
                entity.HasIndex(e => new { e.MetricType, e.Platform, e.Date }).IsUnique();
            });

            // SystemStatistic configuration
            builder.Entity<SystemStatistic>(entity =>
            {
                entity.ToTable("SystemStatistics");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Date)
                    .IsRequired();
                    
                entity.Property(e => e.TotalUsers)
                    .IsRequired();
                    
                entity.Property(e => e.ActiveSubscriptions)
                    .IsRequired();
                    
                entity.Property(e => e.TrialUsers)
                    .IsRequired();
                    
                entity.Property(e => e.SuspendedUsers)
                    .IsRequired();
                    
                entity.Property(e => e.TotalBots)
                    .IsRequired();
                    
                entity.Property(e => e.TotalDocuments)
                    .IsRequired();
                    
                entity.Property(e => e.ActiveDataSources)
                    .IsRequired();
                    
                entity.Property(e => e.ProcessingDataSources)
                    .IsRequired();
                    
                entity.Property(e => e.FailedDataSources)
                    .IsRequired();
                    
                entity.Property(e => e.TotalRevenue)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                    
                entity.Property(e => e.UXPilotApiCost)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                    
                entity.Property(e => e.WhatsAppApiCost)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                    
                entity.Property(e => e.AvgResponseTime)
                    .HasPrecision(10, 2)
                    .IsRequired();
                    
                entity.Property(e => e.ServerUptime)
                    .HasPrecision(5, 2)
                    .IsRequired();
                    
                entity.Property(e => e.DatabaseLoad)
                    .HasPrecision(5, 2)
                    .IsRequired();
                    
                entity.Property(e => e.BotSuccessRate)
                    .HasPrecision(5, 2)
                    .IsRequired();
                    
                entity.Property(e => e.ErrorRate)
                    .HasPrecision(5, 4)
                    .IsRequired();
                    
                entity.Property(e => e.TotalComments)
                    .IsRequired();
                    
                entity.Property(e => e.MessagesSent)
                    .IsRequired();
                    
                entity.Property(e => e.UXPilotApiCalls)
                    .IsRequired();
                    
                entity.Property(e => e.WhatsAppApiCalls)
                    .IsRequired();
                    
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                    
                // الفهارس
                entity.HasIndex(e => e.Date).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // تحديث UpdatedAt للمستخدمين
            foreach (var entry in ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsUpdated();
                }
            }

            // تحديث UpdatedAt لمصادر بيانات AI
            foreach (var entry in ChangeTracker.Entries<AIDataSource>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsUpdated();
                }
            }

            // تحديث UpdatedAt لـ KPIMetrics
            foreach (var entry in ChangeTracker.Entries<KPIMetric>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            // نفس المنطق لـ SaveChanges العادي
            foreach (var entry in ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsUpdated();
                }
            }

            foreach (var entry in ChangeTracker.Entries<AIDataSource>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsUpdated();
                }
            }

            foreach (var entry in ChangeTracker.Entries<KPIMetric>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }
}