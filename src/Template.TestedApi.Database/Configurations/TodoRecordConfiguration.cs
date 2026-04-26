using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.TestedApi.Database.Model;

namespace Template.TestedApi.Database.Configurations;
public class TodoRecordConfiguration : IEntityTypeConfiguration<TodoRecord>
{
    public void Configure(EntityTypeBuilder<TodoRecord> entity)
    {
        entity
            .HasKey(e => e.ItemId);

        entity
            .ToTable("todo_list");

        entity
            .Property(e => e.Version)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnType("xid")
            .HasColumnName("xmin");
    }
}
