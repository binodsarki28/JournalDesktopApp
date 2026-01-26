namespace JournalApplicaton.Model;

public class JournalAnalyticsResult
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int MissedDays { get; set; }

    public Dictionary<string, int> MoodDistribution { get; set; } = new();
    public Dictionary<string, int> TagDistribution { get; set; } = new();

    public List<WordCountTrend> WordCountTrend { get; set; } = new();
}

public class WordCountTrend
{
    public DateTime Date { get; set; }
    public int WordCount { get; set; }
    public string PrimaryMood { get; set; } = string.Empty;
}


