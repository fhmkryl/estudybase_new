using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class KeywordMap : EntityTypeConfiguration<Keyword>
    {
        public KeywordMap()
        {
            // Primary Key
            this.HasKey(t => t.KeywordId);

            // Properties
            this.Property(t => t.Definition)
                .IsRequired()
                .HasMaxLength(400);

            this.Property(t => t.Synonym)
                .HasMaxLength(400);

            this.Property(t => t.Antonym)
                .HasMaxLength(400);

            // Table & Column Mappings
            this.ToTable("Keyword");
            this.Property(t => t.KeywordId).HasColumnName("KeywordId");
            this.Property(t => t.Definition).HasColumnName("Definition");
            this.Property(t => t.Synonym).HasColumnName("Synonym");
            this.Property(t => t.Antonym).HasColumnName("Antonym");
            this.Property(t => t.LanguageId).HasColumnName("LanguageId");
            this.Property(t => t.CreateUserId).HasColumnName("CreateUserId");
            this.Property(t => t.ModifyUserId).HasColumnName("ModifyUserId");
            this.Property(t => t.Approved).HasColumnName("Approved");
            this.Property(t => t.Order).HasColumnName("Order");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.ModifyDate).HasColumnName("ModifyDate");

            // Relationships
            this.HasRequired(t => t.CreateUser)
                .WithMany(t => t.Keywords)
                .HasForeignKey(d => d.CreateUserId);
            this.HasOptional(t => t.ModifyUser)
                .WithMany(t => t.Keywords1)
                .HasForeignKey(d => d.ModifyUserId);
        }
    }
}
