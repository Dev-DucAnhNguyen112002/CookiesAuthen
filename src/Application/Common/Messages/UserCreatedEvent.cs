using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookiesAuthen.Application.Common.Messages;
public class UserCreatedEvent
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
}
