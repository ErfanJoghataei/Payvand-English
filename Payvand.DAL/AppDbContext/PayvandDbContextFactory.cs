using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Payvand.DAL.AppDbContext
{
    public class PayvandDbContextFactory :IDesignTimeDbContextFactory<PayvandDbContext>
    {
        public PayvandDbContext CreateDbContext(string[] args)
        {
            // خواندن appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<PayvandDbContext>();

            var connectionString =
                configuration.GetConnectionString("cnnstring");

            optionsBuilder.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly("Payvand.DAL")
            );

            return new PayvandDbContext(optionsBuilder.Options);
        }
    }
}
