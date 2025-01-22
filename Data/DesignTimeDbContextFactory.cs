using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TaskManagerAPI.Data;

namespace TaskManagerAPI.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();

            optionsBuilder.UseSqlite("Data Source=TaskManager.db");

            return new Context(optionsBuilder.Options);
        }
    }
}
