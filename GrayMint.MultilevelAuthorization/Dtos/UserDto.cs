using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }

        public UserDto(Guid userId)
        {
            UserId = userId;
        }
    }
}
