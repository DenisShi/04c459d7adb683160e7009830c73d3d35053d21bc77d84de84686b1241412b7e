using Microsoft.EntityFrameworkCore;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.Application;

public sealed class PhoneNumberService(
    IPhoneBookDbContext dbContext,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<PhoneNumberResponse>> ListAsync(
        PhoneNumberScope scope,
        CancellationToken cancellationToken)
    {
        var subject = currentUser.Subject;
        return await dbContext.PhoneNumbers
            .AsNoTracking()
            .VisibleIn(scope, subject)
            .OrderByDescending(entry => entry.CreatedAt)
            .ThenByDescending(entry => entry.Id)
            .Select(PhoneNumberResponse.ProjectionFor(subject))
            .ToListAsync(cancellationToken);
    }

    public async Task<PhoneNumberResponse?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var subject = currentUser.Subject;
        return await dbContext.PhoneNumbers
            .AsNoTracking()
            .VisibleIn(PhoneNumberScope.All, subject)
            .Where(entry => entry.Id == id)
            .Select(PhoneNumberResponse.ProjectionFor(subject))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PhoneNumberResponse> CreateAsync(
        CreatePhoneNumberRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var subject = currentUser.Subject;
        var entry = PhoneNumber.Create(
            request.ContactName ?? string.Empty,
            request.Number ?? string.Empty,
            request.Visibility ?? throw new ArgumentException("Visibility is required.", nameof(request)),
            subject,
            currentUser.Username,
            timeProvider);

        dbContext.PhoneNumbers.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
        return PhoneNumberResponse.From(entry, subject);
    }
}
