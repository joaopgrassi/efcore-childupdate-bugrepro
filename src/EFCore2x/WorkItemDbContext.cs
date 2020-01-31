using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCore2x
{
    public class WorkItemDbContext : DbContext
    {
        public DbSet<WorkItem> WorkItems { get; set; }

        public DbSet<WorkItemComment> Comments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=WorkItemTestEF2x;Trusted_Connection=True;MultipleActiveResultSets=true");

            var factory = LoggerFactory.Create(x =>
            {
                x.AddDebug();
                x.SetMinimumLevel(LogLevel.Debug);
            });

            optionsBuilder.UseLoggerFactory(factory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // WorkItemComment
            modelBuilder.Entity<WorkItemComment>().HasKey(x => x.Id);

            modelBuilder.Entity<WorkItemComment>().HasOne(x => x.Parent)
                .WithMany(x => x.Comments).HasForeignKey(x => x.ParentId);

            // WOrkItem
            modelBuilder.Entity<WorkItem>().HasKey(x => x.Id);
        }
    }
}
