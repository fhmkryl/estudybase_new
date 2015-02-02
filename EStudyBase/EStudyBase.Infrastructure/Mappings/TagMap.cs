using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class TagMap : EntityTypeConfiguration<Tag>
    {
        public TagMap()
        {
            // Primary Key
            this.HasKey(t => t.TagId);

            // Properties
            this.Property(t => t.Definition)
                .IsRequired()
                .HasMaxLength(400);

            // Table & Column Mappings
            this.ToTable("Tag");
            this.Property(t => t.TagId).HasColumnName("TagId");
            this.Property(t => t.TagParentId).HasColumnName("TagParentId");
            this.Property(t => t.Definition).HasColumnName("Definition");
            this.Property(t => t.LanguageId).HasColumnName("LanguageId");
            this.Property(t => t.CreateUserId).HasColumnName("CreateUserId");
            this.Property(t => t.ModifyUserId).HasColumnName("ModifyUserId");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.ModifyDate).HasColumnName("ModifyDate");

            // Relationships
            this.HasRequired(t => t.CreateUser)
                .WithMany(t => t.Tags)
                .HasForeignKey(d => d.CreateUserId);
            this.HasOptional(t => t.ModifyUser)
                .WithMany(t => t.Tags1)
                .HasForeignKey(d => d.ModifyUserId);

        }
    }
}
