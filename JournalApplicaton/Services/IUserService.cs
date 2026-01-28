using JournalApplicaton.Common;
using JournalApplicaton.Model;

namespace JournalApplicaton.Services;

public interface IUserService
{
    Task<ServiceResult<UserDisplayModel>> RegisterUserAsync(UserViewModel viewModel);
    Task<ServiceResult<UserDisplayModel>> LoginUserAsync(string username, string password);
}