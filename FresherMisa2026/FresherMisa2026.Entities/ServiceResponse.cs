using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Entities
{
    public record BulkDeleteFailedItem(Guid Id, string Reason);

    public class BulkDeleteResult
    {
        public List<Guid> Succeeded { get; set; } = new();
        public List<BulkDeleteFailedItem> Failed { get; set; } = new();
    }

    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }
        
        public int Code { get; set; }

        public object Data { get; set; }

        public object UserMessage { get; set; }

        public object DevMessage { get; set; }
    }
}
