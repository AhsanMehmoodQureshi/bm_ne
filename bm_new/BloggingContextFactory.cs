using dotnet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace dotnet
{
    public class BloggingContextFactory : IDesignTimeDbContextFactory<Context>
    {
        //  public BloggingContextFactory  (IConfiguration configuration) {
        //     Configuration = configuration;
        // }

        public BloggingContextFactory()
        {

        }
        //  public IConfiguration Configuration { get; }
        public Context CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            /*optionsBuilder.UseSQL(connectionString);*/
            optionsBuilder.UseSqlServer(connectionString);

            return new Context(optionsBuilder.Options);
        }
    }
}