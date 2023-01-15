namespace MultiLevelAuthorization.Dtos;

public class App
{
    public required string AppName { get; init; }
    public Guid? SystemSecureObjectId { get; set; }
}
