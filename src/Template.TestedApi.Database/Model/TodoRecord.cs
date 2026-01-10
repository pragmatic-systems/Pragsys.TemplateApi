namespace Template.TestedApi.Database.Model;
public record TodoRecord(Guid ItemId, string Title, string Description, DateTime DueDate, bool Open = true, DateTime? ClosedDate = null, long Version = 0);