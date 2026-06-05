namespace Template.TestedApi.Database.Model;

public class TodoRecord
{
    public TodoRecord(Guid itemId, string title, string description, DateTime dueDate, bool open = true, DateTime? closedDate = null, long version = 0)
    {
        ItemId = itemId;
        Title = title;
        Description = description;
        DueDate = dueDate;
        Open = open;
        ClosedDate = closedDate;
        Version = version;
    }

    public Guid ItemId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime DueDate { get; set; }

    public bool Open { get; set; }

    public DateTime? ClosedDate { get; set; }

    public long Version { get; set; }
}
