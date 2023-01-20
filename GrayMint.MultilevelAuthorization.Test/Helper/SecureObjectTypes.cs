using MultiLevelAuthorization.Test.Api;
using System;

namespace MultiLevelAuthorization.Test.Helper;
public class SecureObjectTypes
{
    public static SecureObjectType Project { get; } = new() { SecureObjectTypeId = nameof(Project) };
    public static SecureObjectType User { get; } = new() { SecureObjectTypeId = nameof(User) };
    public static SecureObjectType[] All { get; } = { Project, User };
}