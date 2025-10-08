using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Byway.Infrastructure.Data
{
    public class BywayDbContextFactory : IDesignTimeDbContextFactory<BywayDbContext>
    {
        public BywayDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BywayDbContext>();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  
                .AddJsonFile("appsettings.json")               
                .Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            return new BywayDbContext(optionsBuilder.Options);

        }
    }
}
