using System.Reflection;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Domain.Entities;
using CookiesAuthen.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CookiesAuthen.Infrastructure.Data;

// Bạn phải truyền đủ các tham số vào IdentityDbContext
public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    string,
    IdentityUserClaim<string>,
    ApplicationUserRole, // Quan trọng: Truyền class trung gian vào đây
    IdentityUserLogin<string>,
    IdentityRoleClaim<string>,
    IdentityUserToken<string>>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); 

        builder.Entity<Department>(entity =>
        {
            
            entity.HasMany(d => d.Users)
                  .WithOne(u => u.Department)
                  .HasForeignKey(u => u.DepartmentId)
                  .OnDelete(DeleteBehavior.SetNull); 

           
            entity.HasOne(d => d.Manager)
                  .WithMany() 
                  .HasForeignKey(d => d.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationUserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            userRole.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });
    }
}
