using MultiLevelAuthorization.Test.Api;
using System;

namespace MultiLevelAuthorization.Test.Helper;
public class SecureObjectTypes
{
    public static SecureObjectType Project { get; } = new SecureObjectType { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = nameof(Project) };
    public static SecureObjectType User { get; } = new SecureObjectType { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = nameof(User) };

    public static SecureObjectType[] All { get; } = { Project, User };

}