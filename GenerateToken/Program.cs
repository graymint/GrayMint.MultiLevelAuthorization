using MultiLevelAuthorization.Server;

var jwt = JwtTool.CreateSymmetricJwt(Convert.FromBase64String("ctMGlsRMGpn2KY+BtOVgXg=="), AppOptions.AuthIssuer, AppOptions.AuthIssuer,
         "1", null, new[] { "AppCreator" });
Console.WriteLine(jwt);
