using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class LanguageMap : EntityTypeConfiguration<Language>
    {
        public LanguageMap()
        {
            // Primary Key
            this.HasKey(t => t.LanguageId);

            // Properties
            this.Property(t => t.Definition)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Language");
            this.Property(t => t.LanguageId).HasColumnName("LanguageId");
            this.Property(t => t.Definition).HasColumnName("Definition");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
        }
    }
}
