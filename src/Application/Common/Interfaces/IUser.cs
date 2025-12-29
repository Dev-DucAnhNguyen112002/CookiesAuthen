namespace CookiesAuthen.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    bool IsInRole(string roleName);
}
