using System;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test;

public class SecureObjectTypes
{
    public static SecureObjectTypeDto Project { get; } = new SecureObjectTypeDto { SecureObjectTypeId = Guid.Parse("{6FE94D89-632D-40C9-9176-30878F830AEE}"), SecureObjectTypeName = nameof(Project) };
    public static SecureObjectTypeDto User { get; } = new SecureObjectTypeDto { SecureObjectTypeId = Guid.Parse("{CECF8DFF-8ED2-43E4-ACA4-BA5607C5B037}"), SecureObjectTypeName = nameof(User) };

    public static SecureObjectTypeDto[] All { get; } = { Project, User };

}