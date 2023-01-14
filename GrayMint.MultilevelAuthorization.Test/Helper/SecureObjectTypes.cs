using System;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test.Helper;
public class SecureObjectTypes
{
    public static SecureObjectTypeDto Project { get; } = new SecureObjectTypeDto { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = nameof(Project) };
    public static SecureObjectTypeDto User { get; } = new SecureObjectTypeDto { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = nameof(User) };

    public static SecureObjectTypeDto[] All { get; } = { Project, User };

}