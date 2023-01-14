namespace MultiLevelAuthorization.Server;

public class AppOptions
{
    public const string Name = "Authorization Server";
    public const string AuthIssuer = "authorization-server";
    public const string AuthRobotScheme = "Robot";

    public byte[] AuthenticationKey { get; set; } = Array.Empty<byte>();
}