using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace Api.Data
{
    public class ApiDbContextFactory : IDesignTimeDbContextFactory<ApiDbContext>
    {
        public ApiDbContext CreateDbContext(string[] args)
        {
            Env.Load();

            var dbHost = Env.GetString("DB_HOST");
            var dbPort = Env.GetString("DB_PORT");
            var dbUser = Env.GetString("DB_USER");
            var dbPassword = Env.GetString("DB_PASSWORD");
            var dbName = Env.GetString("DB_NAME");

            if (string.IsNullOrWhiteSpace(dbHost))
                throw new Exception("Variáveis de ambiente do banco não foram carregadas.");

            var connectionString =
              $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbName}";

            var optionsBuilder = new DbContextOptionsBuilder<ApiDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApiDbContext(optionsBuilder.Options);
        }
    }
}
