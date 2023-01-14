namespace GrayMint.MultiLevelAuthorization.Models;

public class PermissionModel
{
    public int AppId { get; set; }
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = default!;

    public AppModel? App { get; set; }
    public ICollection<PermissionGroupModel>? PermissionGroups { get; set; }
    public ICollection<PermissionGroupPermissionModel>? PermissionGroupPermissions { get; set; }
}