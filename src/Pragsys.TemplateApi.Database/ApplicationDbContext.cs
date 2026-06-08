using Microsoft.EntityFrameworkCore;
using Pragsys.TemplateApi.Database.Configurations;
using Pragsys.TemplateApi.Database.Model;

namespace Pragsys.TemplateApi.Database;

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
