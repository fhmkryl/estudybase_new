using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class ContentCategoryMap : EntityTypeConfiguration<ContentCategory>
    {
        public ContentCategoryMap()
        {
            // Primary Key
            this.HasKey(t => t.ContentCategoryId);

            // Properties
            this.Property(t => t.Definition)
                .IsRequired()
                .HasMaxLength(400);

            // Table & Column Mappings
            this.ToTable("ContentCategory");
            this.Property(t => t.ContentCategoryId).HasColumnName("ContentCategoryId");
            this.Property(t => t.ContentCategoryParentId).HasColumnName("ContentCategoryParentId");
            this.Property(t => t.Definition).HasColumnName("Definition");
            this.Property(t => t.LanguageId).HasColumnName("LanguageId");
            this.Property(t => t.CreateUserId).HasColumnName("CreateUserId");
            this.Property(t => t.ModifyUserId).HasColumnName("ModifyUserId");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.ModifyDate).HasColumnName("ModifyDate");

        }
    }
}