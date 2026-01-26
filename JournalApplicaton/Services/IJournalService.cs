using JournalApplicaton.Model;
using JournalApplicaton.Entities;
using JournalApplicaton.Common;

namespace JournalApplicaton.Services;

public interface IJournalService
{
    Task<ServiceResult<Journal>> AddOrUpdateJournalAsync(int userId, JournalViewModel model);
    Task<Journal?> GetJournalByDateAsync(int userId, DateTime date);
    Task<(List<JournalDisplayModel> Journals, int TotalCount)> GetAllJournalsByUserAsync(
    int userId, int page = 1, int pageSize = 10);
    Task DeleteJournalAsync(int userId, DateTime date);
    Task<bool> HasJournalForTodayAsync(int userId);
    Task<JournalAnalyticsResult> GetAnalyticsResultAsync(int userId);

    Task<(List<JournalDisplayModel> Journals, int TotalCount)> SearchJournalsAsync(
            int userId,
            string searchText,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize);
    Task<byte[]> GenerateJournalPdfAsync(
    int userId,
    DateTime fromDate,
    DateTime toDate);

}

