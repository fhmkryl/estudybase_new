using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class KeywordTagMap : EntityTypeConfiguration<KeywordTag>
    {
        public KeywordTagMap()
        {
            // Primary Key
            this.HasKey(t => new { t.KeywordId, t.TagId });

            // Properties
            this.Property(t => t.KeywordId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.TagId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("KeywordTag");
            this.Property(t => t.KeywordId).HasColumnName("KeywordId");
            this.Property(t => t.TagId).HasColumnName("TagId");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");

            // Relationships
            this.HasRequired(t => t.Keyword)
                .WithMany(t => t.KeywordTags)
                .HasForeignKey(d => d.KeywordId);
            this.HasRequired(t => t.Tag)
                .WithMany(t => t.KeywordTags)
                .HasForeignKey(d => d.TagId);

        }
    }
}
