using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public partial class EStudyBaseContext : DbContext
    {
        static EStudyBaseContext()
        {
            Database.SetInitializer<EStudyBaseContext>(null);
        }

        public EStudyBaseContext()
            : base("Name=EStudyBaseContext")
        {
        }

        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<KeywordTag> KeywordTags { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<LikeSourceType> LikeSourceTypes { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<EmailLogType> EmailLogTypes { get; set; }
        public DbSet<ContentCategory> ContentCategories { get; set; } 
        public DbSet<webpages_Membership> WebpagesMemberships { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new KeywordMap());
            modelBuilder.Configurations.Add(new KeywordTagMap());
            modelBuilder.Configurations.Add(new LanguageMap());
            modelBuilder.Configurations.Add(new LikeMap());
            modelBuilder.Configurations.Add(new LikeSourceTypeMap());
            modelBuilder.Configurations.Add(new ContentMap());
            modelBuilder.Configurations.Add(new TagMap());
            modelBuilder.Configurations.Add(new UserProfileMap());
            modelBuilder.Configurations.Add(new EmailLogMap());
            modelBuilder.Configurations.Add(new EmailLogTypeMap());
            modelBuilder.Configurations.Add(new ContentCategoryMap());
            modelBuilder.Configurations.Add(new webpages_MembershipMap());
        }
    }
}
