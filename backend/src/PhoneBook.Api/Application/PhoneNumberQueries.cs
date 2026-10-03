using PhoneBook.Api.Domain;

namespace PhoneBook.Api.Application;

public static class PhoneNumberQueries
{
    public static IQueryable<PhoneNumber> VisibleIn(
        this IQueryable<PhoneNumber> source,
        PhoneNumberScope scope,
        string subject)
    {
        var visible = source.Where(VisibilityRules.VisibleTo(subject));
        return scope switch
        {
            PhoneNumberScope.All => visible,
            PhoneNumberScope.Personal => visible.Where(VisibilityRules.PersonalOf(subject)),
            PhoneNumberScope.Shared => visible.Where(VisibilityRules.SharedWithEveryone()),
            _ => throw new ArgumentOutOfRangeException(nameof(scope))
        };
    }
}
