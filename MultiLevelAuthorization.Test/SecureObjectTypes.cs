using System;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test;

public class SecureObjectTypes
{
    public static SecureObjectType Project { get; } = new SecureObjectType { SecureObjectTypeId = Guid.Parse("{6FE94D89-632D-40C9-9176-30878F830AEE}"), SecureObjectTypeName = nameof(Project) };
    public static SecureObjectType User { get; } = new SecureObjectType { SecureObjectTypeId = Guid.Parse("{CECF8DFF-8ED2-43E4-ACA4-BA5607C5B037}"), SecureObjectTypeName = nameof(User) };

    public static SecureObjectType[] All { get; } = { Project, User };

}