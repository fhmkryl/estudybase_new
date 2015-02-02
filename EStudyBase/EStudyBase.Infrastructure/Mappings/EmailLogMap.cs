using System.Data.Entity.ModelConfiguration;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.Infrastructure.Mappings
{
    public class EmailLogMap : EntityTypeConfiguration<EmailLog>
    {
        public EmailLogMap()
        {
            // Primary Key
            this.HasKey(t => t.EmailId);

            // Properties
            this.Property(t => t.Content)
                .IsRequired()
                .HasMaxLength(4000);
            this.Property(t => t.Header)
                .IsRequired()
                .HasMaxLength(400);

            // Table & Column Mappings
            this.ToTable("EmailLog");
            this.Property(t => t.EmailId).HasColumnName("EmailId");
            this.Property(t => t.FromUserId).HasColumnName("FromUserId");
            this.Property(t => t.ToUserId).HasColumnName("ToUserId");
            this.Property(t => t.Content).HasColumnName("Content");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.ModifyDate).HasColumnName("ModifyDate");
            this.Property(t => t.EmailTypeId).HasColumnName("EmailTypeId");
            this.Property(t => t.Read).HasColumnName("Read");
            this.Property(t => t.Deleted).HasColumnName("Deleted");

            this.HasOptional(t => t.FromUser)
                .WithMany(t => t.EmailLogsSent)
                .HasForeignKey(d => d.FromUserId);
            this.HasRequired(t => t.ToUser)
                .WithMany(t => t.EmailLogsReceived)
                .HasForeignKey(d => d.ToUserId);
            this.HasRequired(t => t.EmailLogType)
                .WithMany(t => t.EmailLogs)
                .HasForeignKey(d => d.EmailTypeId);

        }
    }
}