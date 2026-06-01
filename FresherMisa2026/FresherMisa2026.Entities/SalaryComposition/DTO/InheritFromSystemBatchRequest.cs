using System;
using System.Collections.Generic;

namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    public class InheritFromSystemBatchRequest
    {
        public List<Guid> SystemCompositionIds { get; set; } = new();
        public List<Guid> OrganizationIDs { get; set; } = new();
    }
}
