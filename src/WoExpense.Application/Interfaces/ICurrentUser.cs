namespace WoExpense.Application.Interfaces;

public interface ICurrentUser
{
    /// <summary>Id of the authenticated user, or null when the request is anonymous.</summary>
    Guid? UserId { get; }
}
