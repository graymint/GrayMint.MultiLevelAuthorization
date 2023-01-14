using GrayMint.MultiLevelAuthorization.Models;
using Microsoft.EntityFrameworkCore;

namespace GrayMint.MultiLevelAuthorization.Persistence;

// ReSharper disable once PartialTypeWithSinglePart
public partial class AuthDbContext : DbContext
{
    public const string Schema = "auth";

    public virtual DbSet<SecureObjectTypeModel> SecureObjectTypes { get; set; } = default!;
    public virtual DbSet<PermissionGroupModel> PermissionGroups { get; set; } = default!;
    public virtual DbSet<PermissionModel> Permissions { get; set; } = default!;
    public virtual DbSet<PermissionGroupPermissionModel> PermissionGroupPermissions { get; set; } = default!;
    public virtual DbSet<RoleModel> Roles { get; set; } = default!;
    public virtual DbSet<RoleUserModel> RoleUsers { get; set; } = default!;
    public virtual DbSet<SecureObjectModel> SecureObjects { get; set; } = default!;
    public virtual DbSet<SecureObjectRolePermissionModel> SecureObjectRolePermissions { get; set; } = default!;
    public virtual DbSet<SecureObjectUserPermissionModel> SecureObjectUserPermissions { get; set; } = default!;
    public virtual DbSet<AppModel> Apps { get; set; } = default!;

    public IQueryable<SecureObjectModel> SecureObjectHierarchy(int id)
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

        modelBuilder.Entity<AppModel>(entity =>
        {
            entity.Property(e => e.AppId)
                .ValueGeneratedNever();
            entity.HasIndex(e => new { e.AppName })
            .IsUnique();
        });

        modelBuilder.Entity<SecureObjectTypeModel>(entity =>
        {
            entity.HasKey(e => e.SecureObjectTypeId);
            entity.Property(e => e.SecureObjectTypeId)
                .ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.AppId, e.SecureObjectTypeName })
                .IsUnique();
        });

        modelBuilder.Entity<PermissionModel>(entity =>
        {
            entity.HasKey(x => new { x.AppId, x.PermissionId });

            entity.Property(e => e.PermissionId)
                .ValueGeneratedNever();

            entity.HasIndex(e => new { e.AppId, e.PermissionName })
                .IsUnique();

            entity
                .HasMany(p => p.PermissionGroups)
                .WithMany(p => p.Permissions)
                .UsingEntity<PermissionGroupPermissionModel>(
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

        modelBuilder.Entity<PermissionGroupModel>(entity =>
        {
            entity.Property(e => e.PermissionGroupId)
                .ValueGeneratedOnAdd();
            entity.HasKey(x => new { x.PermissionGroupId });
            entity.HasIndex(e => new { e.AppId, e.PermissionGroupName })
                .IsUnique();
        });

        modelBuilder.Entity<PermissionGroupPermissionModel>(entity =>
        {
            entity.HasKey(e => new { e.PermissionGroupId, e.PermissionId });

            entity.HasOne(e => e.App)
                .WithMany(d => d.GroupPermissions)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RoleModel>(entity =>
        {
            entity.HasIndex(e => new { e.AppId, e.RoleName })
                .IsUnique();

        });

        modelBuilder.Entity<RoleUserModel>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.UserId });

            entity.HasOne(e => e.Role)
                .WithMany(d => d.RoleUsers)
                .HasForeignKey(e => new { e.AppId, e.RoleId })
                .HasPrincipalKey(d => new { d.AppId, d.RoleId });
        });

        modelBuilder.Entity<SecureObjectModel>(entity =>
        {
            entity.HasOne(e => e.SecureObjectType)
                .WithMany(d => d.SecureObjects)
                .HasForeignKey(f => new { f.SecureObjectTypeId });
            entity.HasKey(x => x.SecureObjectId);
            entity.Property(x => x.SecureObjectId).
                ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<SecureObjectRolePermissionModel>(entity =>
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

        modelBuilder.Entity<SecureObjectUserPermissionModel>(entity =>
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
            .HasDbFunction(typeof(AuthDbContext).GetMethod(nameof(SecureObjectHierarchy), new[] { typeof(int) })!)
            .HasName(nameof(SecureObjectHierarchy));

        // ReSharper disable once InvocationIsSkipped
        OnModelCreatingPartial(modelBuilder);
    }

    // ReSharper disable once PartialMethodWithSinglePart
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}