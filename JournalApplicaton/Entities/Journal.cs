namespace JournalApplicaton.Entities;

public class Journal
{
    public int JournalId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; }

    public string PrimaryMood { get; set; } = string.Empty;
    public string? SecondaryMood1 { get; set; }
    public string? SecondaryMood2 { get; set; }

    public string? Tags { get; set; }

    public int WordCount { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}


