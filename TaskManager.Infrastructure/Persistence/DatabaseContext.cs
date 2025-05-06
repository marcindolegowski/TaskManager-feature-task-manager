using Accessory.Builder.Outbox.Common;
using Accessory.Builder.Persistence.Core.Common.Logs;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Domain.Task;
using TaskManager.Infrastructure.Persistence.Configurations;

namespace TaskManager.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    protected internal DbSet<AuditTrail>? Audits { get; set; }

    protected internal DbSet<Task>? Users { get; set; }

    protected internal DbSet<OutboxMessage>? OutboxMessages { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AuditConfiguration).Assembly);
    }


}