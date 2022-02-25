﻿using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class Role
{
    public short AppId { get; init; }
    public Guid RoleId { get; set; }
    public Guid OwnerId { get; set; }
    public string RoleName { get; set; } = null!;
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual App? App { get; set; }
    public virtual ICollection<SecureObjectRolePermission>? RolePermissions { get; set; }
    public virtual ICollection<RoleUser>? RoleUsers { get; set; }

}