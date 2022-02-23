using Microsoft.EntityFrameworkCore;


namespace MultiLevelAuthorization.Models;

public abstract class AuthDbContext : DbContext
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

    public IQueryable<SecureObject> SecureObjectHierarchy(Guid id)
        => FromExpression(() => SecureObjectHierarchy(id));

    protected AuthDbContext()
    {
    }

    protected AuthDbContext(DbContextOptions options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthApp>(entity =>
        {
            entity.ToTable("Apps");
            entity.HasKey(e => e.AppId);
        });

        modelBuilder.Entity<SecureObjectType>(entity =>
        {
            entity.ToTable(nameof(SecureObjectTypes), Schema);
            entity.Property(e => e.SecureObjectTypeId)
                .ValueGeneratedNever();

            entity.HasIndex(e => e.SecureObjectTypeName)
                .IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable(nameof(Permissions), Schema);

                entity.Property(e => e.PermissionCode)
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
                            .HasForeignKey(pt => pt.PermissionGroupId)
                            .OnDelete(DeleteBehavior.NoAction),
                        j => j
                            .HasOne(pt => pt.Permission)
                            .WithMany(p => p.PermissionGroupPermissions)
                            .HasForeignKey(pt => pt.PermissionCode)
                    );
            });

        modelBuilder.Entity<PermissionGroup>(entity =>
        {
            entity.ToTable(nameof(PermissionGroups), Schema);

            entity.Property(e => e.PermissionGroupId)
                .ValueGeneratedNever();

            entity.HasIndex(e => e.PermissionGroupName)
                .IsUnique();
        });

        modelBuilder.Entity<PermissionGroupPermission>(entity =>
        {
            entity.ToTable(nameof(PermissionGroupPermissions), Schema);
            entity.HasKey(e => new { e.PermissionGroupId, PermissionId = e.PermissionCode });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable(nameof(Roles), Schema);
        });

        modelBuilder.Entity<RoleUser>(entity =>
        {
            entity.ToTable(nameof(RoleUsers), Schema);

            entity.HasKey(e => new { e.UserId, e.RoleId });
        });

        modelBuilder.Entity<SecureObject>(entity =>
        {
            entity.ToTable(nameof(SecureObjects), Schema);
        });

        modelBuilder.Entity<SecureObjectRolePermission>(entity =>
        {
            entity.ToTable(nameof(SecureObjectRolePermissions), Schema);
            entity.HasKey(e => new { e.SecureObjectId, e.RoleId, e.PermissionGroupId });

            entity.HasOne(e => e.Role)
                .WithMany(d => d.RolePermissions)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.PermissionGroup)
                .WithMany(d => d.RolePermissions)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SecureObjectUserPermission>(entity =>
        {
            entity.ToTable(nameof(SecureObjectUserPermissions), Schema);
            entity.HasKey(e => new { e.SecureObjectId, UsedId = e.UserId, e.PermissionGroupId });

            entity.HasOne(e => e.PermissionGroup)
                .WithMany(d => d.UserPermissions)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // functions
        modelBuilder
            .HasDbFunction(typeof(AuthDbContext).GetMethod(nameof(SecureObjectHierarchy), new[] { typeof(Guid) })!)
            .HasSchema(Schema)
            .HasName(nameof(SecureObjectHierarchy));
    }
}