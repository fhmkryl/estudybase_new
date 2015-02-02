using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class LikeMap : EntityTypeConfiguration<Like>
    {
        public LikeMap()
        {
            // Primary Key
            this.HasKey(t => new { t.KeywordId, t.SourceId, t.Status, t.SourceTypeId, t.CreateDate });

            // Properties
            this.Property(t => t.KeywordId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.SourceId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.SourceTypeId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("Like");
            this.Property(t => t.KeywordId).HasColumnName("KeywordId");
            this.Property(t => t.SourceId).HasColumnName("SourceId");
            this.Property(t => t.LikeUserId).HasColumnName("LikeUserId");
            this.Property(t => t.Status).HasColumnName("Status");
            this.Property(t => t.SourceTypeId).HasColumnName("SourceTypeId");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");

            // Relationships
            this.HasRequired(t => t.Keyword)
                .WithMany(t => t.Likes)
                .HasForeignKey(d => d.KeywordId);
            this.HasRequired(t => t.LikeSourceType)
                .WithMany(t => t.Likes)
                .HasForeignKey(d => d.SourceTypeId);
            this.HasRequired(t => t.CreateUser)
                .WithMany(t => t.Likes)
                .HasForeignKey(d => d.LikeUserId);

        }
    }
}
