namespace JournalApplicaton.Entities;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password {  get; set; } = string.Empty;

    public ICollection<Journal> Journals { get; set; } = new List<Journal>();
}
