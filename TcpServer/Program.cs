using Microsoft.Extensions.Configuration;
using TcpServer;

string connectionString = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json").Build()
    .GetConnectionString("Default")!;
    
//AppDbContext context = new(o => o.UserSqlServer(connectionString));    