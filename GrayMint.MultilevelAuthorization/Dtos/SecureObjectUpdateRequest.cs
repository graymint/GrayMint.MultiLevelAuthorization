using GrayMint.Common.Utils;

namespace MultiLevelAuthorization.Dtos;

public class SecureObjectUpdateRequest
{
    public Patch<string> ParentSecureObjectTypeId { get; set; } = default!;
    public Patch<string> ParentSecureObjectId { get; set; } = default!;
}