namespace WoExpense.Domain.Entities;

public class Expense : BaseEntity   // gets Id, CreatedAt/By, UpdatedAt/By, IsDeleted
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime SpentOn { get; set; }
    public Guid UserId { get; set; }   
    public User User { get; set; } = null!;
}
