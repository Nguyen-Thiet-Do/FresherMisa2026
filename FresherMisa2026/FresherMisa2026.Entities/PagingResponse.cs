using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Entities
{
    public class PagingResponse<T>
    {
        public long Total { get; set; }
        public long PageSize { get; set; }

        public long CurrentPage { get; set; }
        
        public long PageCount { get; set; }

        public List<T> Data { get; set; }
    }
}
