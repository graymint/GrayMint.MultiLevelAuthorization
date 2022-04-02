namespace MultiLevelAuthorization.Models;

public class PermissionGroupPermission
{
    public int AppId { get; set; }
    public Guid PermissionGroupId { get; set; }
    public int PermissionId { get; set; }
    public virtual PermissionGroup? PermissionGroup { get; set; }
    public virtual Permission? Permission { get; set; }
    public virtual App? App { get; set; }

}