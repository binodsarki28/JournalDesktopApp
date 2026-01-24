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

    Task<bool> DeleteJournalAsync(int userId, int journalId);

    Task<bool> HasJournalForTodayAsync(int userId);

}

