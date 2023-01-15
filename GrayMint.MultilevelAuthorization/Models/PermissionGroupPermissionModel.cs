namespace MultiLevelAuthorization.Models;

public class PermissionGroupPermissionModel
{
    public int AppId { get; set; }
    public int PermissionGroupId { get; set; }
    public int PermissionId { get; set; }

    public virtual PermissionGroupModel? PermissionGroup { get; set; }
    public virtual PermissionModel? Permission { get; set; }
    public virtual AppModel? App { get; set; }

}