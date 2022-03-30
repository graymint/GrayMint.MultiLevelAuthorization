using Microsoft.EntityFrameworkCore;


namespace MultiLevelAuthorization.Models;

// ReSharper disable once PartialTypeWithSinglePart
public partial class AuthDbContext : DbContext
{
    public const string Schema = "auth";

    public virtual DbSet<SecureObjectType> SecureObjectTypes { get; set; } = default!;
    public virtual DbSet<PermissionGroup> PermissionGroups { get; set; } = default!;
    public virtual DbSet<Permission> Permissions { get; set; } = default!;
    public virtual DbSet<PermissionGroupPermission> PermissionGroupPermissions { get; set; } = default!;
    public virtual DbSet<Role> Roles { get; set; } = default!;
    public virtual DbSet<RoleUser> RoleUsers { get; set; } = default!;
    public virtual DbSet<SecureObject> SecureObjects { get; set; } = default!;
    public virtual DbSet<SecureObjectRolePermission> SecureObjectRolePermissions { get; set; } = default!;
    public virtual DbSet<SecureObjectUserPermission> SecureObjectUserPermissions { get; set; } = default!;
    public virtual DbSet<App> Apps { get; set; } = default!;

    public IQueryable<SecureObject> SecureObjectHierarchy(Guid id)
        => FromExpression(() => SecureObjectHierarchy(id));

    public AuthDbContext()
    {
    }

    public AuthDbContext(DbContextOptions options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<App>(entity =>
        {
            entity.Property(e => e.AppId)
                .ValueGeneratedNever();
        });

        modelBuilder.Entity<SecureObjectType>(entity =>
        {
            entity.Property(e => e.SecureObjectTypeId)
                .ValueGeneratedNever();

            entity.HasIndex(e => new {e.AppId, e.SecureObjectTypeName})
                .IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(x => new { x.AppId, x.PermissionId });

            entity.Property(e => e.PermissionId)
                .ValueGeneratedNever();

            entity.HasIndex(e => new { e.AppId, e.PermissionName })
                .IsUnique();

            entity
                .HasMany(p => p.PermissionGroups)
                .WithMany(p => p.Permissions)
                .UsingEntity<PermissionGroupPermission>(
                    j => j
                        .HasOne(pt => pt.PermissionGroup)
                        .WithMany(t => t.PermissionGroupPermissions)
                        .HasPrincipalKey(p => new { p.AppId, p.PermissionGroupId })
                        .HasForeignKey(pt => new { pt.AppId, pt.PermissionGroupId })
                        .OnDelete(DeleteBehavior.NoAction),
                    j => j
                        .HasOne(pt => pt.Permission)
                        .WithMany(p => p.PermissionGroupPermissions)
                        .HasPrincipalKey(p => new { p.AppId, p.PermissionId })
                        .HasForeignKey(f => new { f.AppId, f.PermissionId })
                );
        });

        modelBuilder.Entity<PermissionGroup>(entity =>
        {
            entity.Property(e => e.PermissionGroupId)
                .ValueGeneratedNever();

            entity.HasIndex(e => new { e.AppId, e.PermissionGroupName })
                .IsUnique();
        });

        modelBuilder.Entity<PermissionGroupPermission>(entity =>
        {
            entity.HasKey(e => new { e.PermissionGroupId, e.PermissionId });

            entity.HasOne(e => e.App)
                .WithMany(d => d.GroupPermissions)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => new { e.AppId, e.RoleName })
                .IsUnique();

        });

        modelBuilder.Entity<RoleUser>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.UserId });

            entity.HasOne(e => e.Role)
                .WithMany(d => d.RoleUsers)
                .HasForeignKey(e => new { e.AppId, e.RoleId })
                .HasPrincipalKey(d => new { d.AppId, d.RoleId });
        });

        modelBuilder.Entity<SecureObject>(entity =>
        {
            entity.HasOne(e => e.SecureObjectType)
                .WithMany(d => d.SecureObjects)
                .HasPrincipalKey(p => new { p.AppId, p.SecureObjectTypeId })
                .HasForeignKey(f => new { f.AppId, f.SecureObjectTypeId });

        });

        modelBuilder.Entity<SecureObjectRolePermission>(entity =>
        {
            entity.HasKey(e => new { e.SecureObjectId, e.RoleId, e.PermissionGroupId });

            entity.HasOne(e => e.Role)
                .WithMany(d => d.RolePermissions)
                .HasForeignKey(e => new { e.AppId, e.RoleId })
                .HasPrincipalKey(p => new { p.AppId, p.RoleId })
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.PermissionGroup)
                .WithMany(d => d.RolePermissions)
                .HasForeignKey(e => new { e.AppId, e.PermissionGroupId })
                .HasPrincipalKey(d => new { d.AppId, d.PermissionGroupId })
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.SecureObject)
                .WithMany(d => d.RolePermissions)
                .HasForeignKey(e => new { e.AppId, e.SecureObjectId })
                .HasPrincipalKey(d => new { d.AppId, d.SecureObjectId })
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SecureObjectUserPermission>(entity =>
        {
            entity.HasKey(e => new { e.SecureObjectId, UsedId = e.UserId, e.PermissionGroupId });

            entity.HasOne(e => e.PermissionGroup)
                .WithMany(d => d.UserPermissions)
                .HasForeignKey(e => new { e.AppId, e.PermissionGroupId })
                .HasPrincipalKey(d => new { d.AppId, d.PermissionGroupId })
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.SecureObject)
                .WithMany(d => d.UserPermissions)
                .HasForeignKey(e => new { e.AppId, e.SecureObjectId })
                .HasPrincipalKey(d => new { d.AppId, d.SecureObjectId })
                .OnDelete(DeleteBehavior.NoAction);

        });

        // functions
        modelBuilder
            .HasDbFunction(typeof(AuthDbContext).GetMethod(nameof(SecureObjectHierarchy), new[] { typeof(Guid) })!)
            .HasName(nameof(SecureObjectHierarchy));

        // ReSharper disable once InvocationIsSkipped
        OnModelCreatingPartial(modelBuilder);
    }

    // ReSharper disable once PartialMethodWithSinglePart
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}