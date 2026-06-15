using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public class AgentRunConfiguration : IEntityTypeConfiguration<AgentRun>
{
    public void Configure(EntityTypeBuilder<AgentRun> builder)
    {
        builder.ToTable("AgentRuns");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.TaskId);

        builder.Property(a => a.TaskId).IsRequired();
        builder.Property(a => a.Status).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Branch).HasMaxLength(255);
        builder.Property(a => a.PrUrl).HasMaxLength(1000);
        builder.Property(a => a.CostUsd).HasColumnType("decimal(10,4)");
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
    }
}
