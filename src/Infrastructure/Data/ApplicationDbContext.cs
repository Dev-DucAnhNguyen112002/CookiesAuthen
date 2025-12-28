using System.Reflection;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Domain.Entities;
using CookiesAuthen.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CookiesAuthen.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public new DbSet<TEntity> Set<TEntity>() where TEntity : class
    {
        return base.Set<TEntity>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>()
        .HasOne(u => u.Department)
        .WithMany() // Lưu ý: Để trống (WithMany()) vì bên Department không có List Members
        .HasForeignKey(u => u.DepartmentId)
        .OnDelete(DeleteBehavior.SetNull);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
