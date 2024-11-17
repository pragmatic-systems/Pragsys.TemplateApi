namespace Template.DbApi.Model;
public record TodoRecord(Guid ItemId, string Title, string Description, DateTime DueDate, bool Open = true, DateTime? ClosedDate = null);