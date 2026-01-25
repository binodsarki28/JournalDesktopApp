namespace JournalApplicaton.Model;
public class JournalStatsModel
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int ThisMonthEntries { get; set; }
    public double AverageWordCountThisMonth { get; set; }
}

