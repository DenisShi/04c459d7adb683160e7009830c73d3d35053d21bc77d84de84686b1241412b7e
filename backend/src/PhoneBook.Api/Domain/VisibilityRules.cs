using System.Linq.Expressions;

namespace PhoneBook.Api.Domain;

public static class VisibilityRules
{
    public static Expression<Func<PhoneNumber, bool>> VisibleTo(string subject) =>
        entry => entry.Visibility == Visibility.Shared || entry.OwnerId == subject;

    public static Expression<Func<PhoneNumber, bool>> PersonalOf(string subject) =>
        entry => entry.OwnerId == subject && entry.Visibility == Visibility.Personal;

    public static Expression<Func<PhoneNumber, bool>> SharedWithEveryone() =>
        entry => entry.Visibility == Visibility.Shared;
}
