using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once PartialTypeWithSinglePart
namespace MultiLevelAuthorization.Server.Models;

public partial class ApplicationDbContext : DbContext
{
    public virtual DbSet<App> Apps { get; set; } = default!;

    public bool DebugMode { get; set; }

    private IDbContextTransaction? _transaction;

    public ApplicationDbContext()
    {
    }

    public async Task<ApplicationDbContext> WithNoLock()
    {
        if (Database.CurrentTransaction == null)
            _transaction = await Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted);

        return this;
    }

    public override void Dispose()
    {
        _transaction?.Dispose();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_transaction != null)
            await _transaction.DisposeAsync();

        await base.DisposeAsync();
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HavePrecision(0);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_100_CI_AS_SC_UTF8");

        modelBuilder.Entity<App>(entity =>
        {
            entity.HasIndex(e => e.AppName).IsUnique();
        });

        // ReSharper disable once InvocationIsSkipped
        OnModelCreatingPartial(modelBuilder);
    }

    // ReSharper disable once PartialMethodWithSinglePart
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}