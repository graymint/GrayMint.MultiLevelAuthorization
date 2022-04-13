using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.DTOs
{
    public class RoleDto
    {
        public Guid RoleId { get; set; } 
        public string RoleName { get; set; }

        public RoleDto(Guid roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
        }
    }
}
