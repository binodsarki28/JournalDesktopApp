using JournalApplicaton.Common;
using JournalApplicaton.Data;
using JournalApplicaton.Entities;
using JournalApplicaton.Model;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Text.RegularExpressions;
using Colors = QuestPDF.Helpers.Colors;

namespace JournalApplicaton.Services;

public class JournalService : IJournalService
{
    private readonly AppDbContext _context;

    public JournalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<Journal>> AddOrUpdateJournalAsync(int userId, JournalViewModel model)
    {
        try
        {
            // Prevent future date
            if (model.EntryDate > DateTime.Today)
                return ServiceResult<Journal>.FailureResult("Cannot create journal for future dates");

            // Check if journal exists for this user & date
            var journal = await _context.Journals
                .FirstOrDefaultAsync(j => j.UserId == userId && j.EntryDate.Date == model.EntryDate.Date);

            if (journal == null)
            {
                // Create new
                journal = new Journal
                {
                    UserId = userId,
                    Title = model.Title,
                    Content = model.Content,
                    EntryDate = model.EntryDate.Date,
                    PrimaryMood = model.PrimaryMood,
                    SecondaryMoods = model.SecondaryMoods,
                    Tags = model.Tags,
                    WordCount = CountWords(model.Content),
                    UpdatedAt = DateTime.Now
                };

                _context.Journals.Add(journal);
            }
            else
            {
                // Update existing
                journal.Title = model.Title;
                journal.Content = model.Content;
                journal.PrimaryMood = model.PrimaryMood;
                journal.SecondaryMoods = model.SecondaryMoods;
                journal.Tags = model.Tags;
                journal.WordCount = CountWords(model.Content);
                journal.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return ServiceResult<Journal>.SuccessResult(journal);
        }
        catch (Exception ex)
        {
            return ServiceResult<Journal>.FailureResult($"Error saving journal: {ex.Message}");
        }
    }

    public async Task<Journal?> GetJournalByDateAsync(int userId, DateTime date)
    {
        return await _context.Journals
            .FirstOrDefaultAsync(j => j.UserId == userId && j.EntryDate.Date == date.Date);
    }

    private int CountWords(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length;
    }

    public async Task<(List<JournalDisplayModel> Journals, int TotalCount)> GetAllJournalsByUserAsync(
    int userId, int page = 1, int pageSize = 10)
    {
        // Only fetch journals of this user
        var query = _context.Journals
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.EntryDate);

        // Get total count for pagination
        int totalCount = await query.CountAsync();

        // Apply pagination
        var journals = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JournalDisplayModel
            {
                JournalId = j.JournalId,
                EntryDate = j.EntryDate,
                Title = j.Title,
                PrimaryMood = j.PrimaryMood,
                SecondaryMoods = j.SecondaryMoods,
                Tags = j.Tags,
                WordCount = j.WordCount
            })
            .ToListAsync();

        return (journals, totalCount);
    }

    public async Task DeleteJournalAsync(int userId, DateTime date)
    {
        var journal = await _context.Journals
            .FirstOrDefaultAsync(j =>
                j.UserId == userId &&
                j.EntryDate.Date == date.Date);

        if (journal == null)
            return;

        _context.Journals.Remove(journal);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasJournalForTodayAsync(int userId)
    {
        return await _context.Journals.AnyAsync(j =>
            j.UserId == userId &&
            j.EntryDate.Date == DateTime.Today);
    }

    public async Task<JournalAnalyticsResult> GetAnalyticsResultAsync(int userId)
    {
        var journals = await _context.Journals
            .Where(j => j.UserId == userId)
            .OrderBy(j => j.EntryDate)
            .ToListAsync();

        if (!journals.Any())
            return new JournalAnalyticsResult();

        var dates = journals
            .Select(j => j.EntryDate.Date)
            .Distinct()
            .ToList();

        return new JournalAnalyticsResult
        {
            CurrentStreak = CalculateCurrentStreak(dates),
            LongestStreak = CalculateLongestStreak(dates),
            MissedDays = CalculateMissedDays(dates),
            MoodDistribution = CalculateMoodDistribution(journals),
            TagDistribution = CalculateTagDistribution(journals),
            WordCountTrend = CalculateWordCountTrend(journals)
        };
    }

    // ======================= STREAK LOGIC =======================
    private int CalculateCurrentStreak(List<DateTime> dates)
    {
        var set = dates.ToHashSet();
        var today = DateTime.Today;

        // If today not written → streak = 0
        if (!set.Contains(today))
            return 0;

        int streak = 0;
        while (set.Contains(today.AddDays(-streak)))
            streak++;

        return streak;
    }

    private int CalculateLongestStreak(List<DateTime> dates)
    {
        var ordered = dates.OrderBy(d => d).ToList();
        int longest = 0;
        int current = 1;

        for (int i = 1; i < ordered.Count; i++)
        {
            if ((ordered[i] - ordered[i - 1]).Days == 1)
            {
                current++;
            }
            else
            {
                longest = Math.Max(longest, current);
                current = 1;
            }
        }

        return Math.Max(longest, current);
    }

    private int CalculateMissedDays(List<DateTime> dates)
    {
        var firstDate = dates.Min();
        var totalDays = (DateTime.Today - firstDate).Days + 1;

        return totalDays - dates.Count;
    }

    // ======================= MOOD ANALYTICS =======================
    private Dictionary<string, int> CalculateMoodDistribution(List<Journal> journals)
    {
        return journals
            .Select(j => j.PrimaryMood)
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // ======================= TAG ANALYTICS =======================
    private Dictionary<string, int> CalculateTagDistribution(List<Journal> journals)
    {
        return journals
            .SelectMany(j => j.Tags)
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // ======================= WORD COUNT TREND =======================
    private List<WordCountTrend> CalculateWordCountTrend(List<Journal> journals)
    {
        return journals
            .OrderBy(j => j.EntryDate)
            .Select(j => new WordCountTrend
            {
                Date = j.EntryDate,
                WordCount = j.WordCount,
                PrimaryMood = j.PrimaryMood
            })
            .ToList();
    }

    public async Task<(List<JournalDisplayModel>, int)>
     SearchJournalsAsync(
         int userId,
         string title,
         string mood,
         string tag,
         DateTime? fromDate,
         DateTime? toDate,
         int page,
         int pageSize)
    {
        var query = _context.Journals
            .Where(j => j.UserId == userId);

        title = title?.ToLower() ?? "";
        mood = mood?.ToLower() ?? "";
        tag = tag?.ToLower() ?? "";

        // 🔍 TITLE
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(j =>
                j.Title.ToLower().Contains(title));

        // 😊 MOOD
        if (!string.IsNullOrWhiteSpace(mood))
            query = query.Where(j =>
                j.PrimaryMood.ToLower() == mood);

        // 🏷️ TAG
        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(j =>
                j.Tags.Any(t => t.ToLower().Contains(tag)));

        // 📅 DATE FILTER
        if (fromDate.HasValue)
            query = query.Where(j => j.EntryDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(j => j.EntryDate <= toDate.Value);

        var totalCount = await query.CountAsync();

        var journals = await query
            .OrderByDescending(j => j.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JournalDisplayModel
            {
                JournalId = j.JournalId,
                EntryDate = j.EntryDate,
                Title = j.Title,
                Content = j.Content,
                PrimaryMood = j.PrimaryMood,
                SecondaryMoods = j.SecondaryMoods,
                Tags = j.Tags,
                WordCount = j.WordCount
            })
            .ToListAsync();

        return (journals, totalCount);
    }


    public async Task<byte[]> GenerateJournalPdfAsync(
    int userId,
    DateTime fromDate,
    DateTime toDate)
    {
        var journals = await _context.Journals
            .Where(j =>
                j.UserId == userId &&
                j.EntryDate >= fromDate &&
                j.EntryDate <= toDate)
            .OrderBy(j => j.EntryDate)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                // ===== HEADER =====
                page.Header().Column(h =>
                {
                    h.Item().Text("Journal Report")
                        .SemiBold()
                        .FontSize(18)
                        .AlignCenter();

                    h.Item().PaddingTop(5).Text(
                        $"{fromDate:dd MMM yyyy} - {toDate:dd MMM yyyy}")
                        .AlignCenter()
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });

                // ===== CONTENT =====
                page.Content().PaddingTop(20).Column(col =>
                {
                    foreach (var j in journals)
                    {
                        col.Item().BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .PaddingBottom(15)
                            .PaddingTop(10)
                            .Column(c =>
                            {
                                c.Item().Text(j.EntryDate.ToString("dd MMM yyyy"))
                                    .SemiBold()
                                    .FontSize(12);

                                c.Item().PaddingTop(3)
                                    .Text(j.Title)
                                    .SemiBold()
                                    .FontSize(13);

                                c.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text($"Mood: {j.PrimaryMood}");
                                    r.RelativeItem().AlignRight()
                                        .Text($"Words: {j.WordCount}");
                                });

                                c.Item().PaddingTop(8)
                                    .Text(CleanHtml(j.Content))
                                    .LineHeight(1.4f);
                            });
                    }
                });

                // ===== FOOTER =====
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ");
                    text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm"))
                        .SemiBold();
                });
            });
        });

        return document.GeneratePdf();
    }

private string CleanHtml(string html)
{
    if (string.IsNullOrWhiteSpace(html))
        return string.Empty;

    // Convert block tags to line breaks
    html = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
    html = Regex.Replace(html, @"</p>|</li>|</ul>|</ol>", "\n");

    // Remove all remaining HTML tags
    html = Regex.Replace(html, "<.*?>", string.Empty);

    // Decode HTML entities
    html = System.Net.WebUtility.HtmlDecode(html);

    return html.Trim();
}

}
