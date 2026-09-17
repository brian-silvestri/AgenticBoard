using AgenticBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgenticBoard.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PerformedByName)
            .HasMaxLength(100);

        builder.HasOne(a => a.Project)
            .WithMany(p => p.AuditLogs)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.PerformedBy)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.PerformedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.ProjectId, a.Timestamp });
    }
}
