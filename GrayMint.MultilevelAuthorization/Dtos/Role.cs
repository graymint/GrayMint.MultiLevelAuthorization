namespace GrayMint.MultiLevelAuthorization.Dtos;

public class Role
{
    public required Guid RoleId { get; init; }
    public required string RoleName { get; init; }
}