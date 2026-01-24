namespace JournalApplicaton.Model;

public class JournalDisplayModel
{
    public int JournalId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
    public string PrimaryMood { get; set; } = string.Empty;
    public string? SecondaryMood1 { get; set; }
    public string? SecondaryMood2 { get; set; }
    public string? Tags { get; set; }

    public int WordCount { get; set; }
}
