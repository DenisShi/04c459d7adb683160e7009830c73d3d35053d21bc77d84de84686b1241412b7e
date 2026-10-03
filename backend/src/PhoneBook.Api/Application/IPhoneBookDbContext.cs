using Microsoft.EntityFrameworkCore;
using PhoneBook.Api.Domain;

namespace PhoneBook.Api.Application;

public interface IPhoneBookDbContext
{
    DbSet<PhoneNumber> PhoneNumbers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
