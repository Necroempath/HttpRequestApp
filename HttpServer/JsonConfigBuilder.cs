using Microsoft.Extensions.Configuration;

namespace TcpServer;

public class JsonConfigBuilder
{
    public static string GetConnectionString()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json").Build()
            .GetConnectionString("Default")!;
    }
}