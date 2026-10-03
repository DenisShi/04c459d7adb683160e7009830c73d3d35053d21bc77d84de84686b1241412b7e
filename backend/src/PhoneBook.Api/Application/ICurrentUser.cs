namespace PhoneBook.Api.Application;

public interface ICurrentUser
{
    string Subject { get; }

    string Username { get; }
}
