using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.Domain.Task;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(s => s.Id).IsClustered(false);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property<long>("ClusteredId").UseIdentityColumn();
        builder.HasIndex("ClusteredId")
            .IsClustered();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(1800);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.CreationDate)
            .IsRequired();

        builder.Property(a => a.LastUpdateDate)
            .IsRequired();
    }
}