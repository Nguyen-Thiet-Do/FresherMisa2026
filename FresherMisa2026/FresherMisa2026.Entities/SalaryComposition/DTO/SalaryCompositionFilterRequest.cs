using FresherMisa2026.Entities.Enums;
using System;

namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    /// <summary>
    /// Request lọc danh sách thành phần lương
    /// Created By: Nguyen Thiet Do (2026-05-26)
    /// </summary>
    public class SalaryCompositionFilterRequest
    {
        public string? Search { get; set; }

        public List<Guid>? OrganizationIDs { get; set; }

        public Guid? ComponentTypeID { get; set; }

        public SalaryNature? Nature { get; set; }

        public SalaryCompositionStatus? Status { get; set; }

        public SalaryCompositionSource? Source { get; set; }

        public int PageSize { get; set; } = 10;

        public int PageIndex { get; set; } = 1;
    }
}
