using Microsoft.EntityFrameworkCore;
using Pragmatic.TemplateApi.Database.Configurations;
using Pragmatic.TemplateApi.Database.Model;

namespace Pragmatic.TemplateApi.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public virtual DbSet<TodoRecord> TodoRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TodoRecordConfiguration());
    }
}
