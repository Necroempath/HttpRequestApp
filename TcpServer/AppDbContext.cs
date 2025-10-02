using Microsoft.EntityFrameworkCore;

namespace TcpServer;

public class AppDbContext : DbContext
{
    public DbSet<Car> Cars { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
}