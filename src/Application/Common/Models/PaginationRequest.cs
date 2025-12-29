using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookiesAuthen.Application.Common.Models;
public abstract record PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public string? SortBy { get; set; }
    public bool? IsDesc { get; set; }
    public PaginationRequest()
    {
        PageNumber = 1;
        PageSize = 20;
    }
    public PaginationRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize;
    }
}
