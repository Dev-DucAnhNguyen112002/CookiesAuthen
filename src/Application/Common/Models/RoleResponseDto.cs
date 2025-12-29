using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookiesAuthen.Application.Common.Models;
public record RoleResponseDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
}
