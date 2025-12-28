namespace CookiesAuthen.Domain.Entities;
public class Department : BaseAuditableEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
}
