using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VibeCode.Main.Data
{
    public class VibeCodeDbContextFactory : IDesignTimeDbContextFactory<VibeCodeDbContext>
    {
        public VibeCodeDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<VibeCodeDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new VibeCodeDbContext(optionsBuilder.Options, null!);
        }
    }
}
