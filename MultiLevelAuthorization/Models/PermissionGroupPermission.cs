namespace MultiLevelAuthorization.Models;

public class PermissionGroupPermission
{
    public Guid PermissionGroupId { get; set; }
    public int PermissionCode { get; set; }

    public virtual PermissionGroup? PermissionGroup { get; set; }
    public virtual Permission? Permission { get; set; }
    public virtual AuthApp? App { get; set; }

}