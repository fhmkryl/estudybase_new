using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class ContentMap : EntityTypeConfiguration<Content>
    {
        public ContentMap()
        {
            // Primary Key
            this.HasKey(t => t.ContentId);

            // Properties
            this.Property(t => t.Url)
                .HasMaxLength(1000);

            this.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(4000);

            // Table & Column Mappings
            this.ToTable("Content");
            this.Property(t => t.ContentId).HasColumnName("ContentId");
            this.Property(t => t.KeywordId).HasColumnName("KeywordId");
            this.Property(t => t.LanguageId).HasColumnName("LanguageId");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.FileSize).HasColumnName("FileSize");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.ContentCategoryId).HasColumnName("ContentCategoryId");
            this.Property(t => t.ContentType).HasColumnName("ContentType");
            this.Property(t => t.CreateUserId).HasColumnName("CreateUserId");
            this.Property(t => t.ModifyUserId).HasColumnName("ModifyUserId");
            this.Property(t => t.Approved).HasColumnName("Approved");
            this.Property(t => t.LikeCount).HasColumnName("LikeCount");
            this.Property(t => t.DislikeCount).HasColumnName("DislikeCount");
            this.Property(t => t.Complaint).HasColumnName("Complaint");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.ModifyDate).HasColumnName("ModifyDate");

            // Relationships
            this.HasRequired(t => t.ContentCategory)
                .WithMany(t => t.Contents)
                .HasForeignKey(d => d.ContentCategoryId);
            this.HasRequired(t => t.Keyword)
                .WithMany(t => t.Contents)
                .HasForeignKey(d => d.KeywordId);
            this.HasRequired(t => t.CreateUser)
                .WithMany(t => t.Contents)
                .HasForeignKey(d => d.CreateUserId);
            this.HasOptional(t => t.ModifyUser)
                .WithMany(t => t.Contents1)
                .HasForeignKey(d => d.ModifyUserId);
        }
    }
}