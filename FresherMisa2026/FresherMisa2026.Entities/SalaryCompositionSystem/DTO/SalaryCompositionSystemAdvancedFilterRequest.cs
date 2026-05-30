using FresherMisa2026.Entities.AdvancedFilter;

namespace FresherMisa2026.Entities.SalaryCompositionSystem.DTO
{
    public class SalaryCompositionSystemAdvancedFilterRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Sort { get; set; }
        public string? Search { get; set; }
        public Guid? ComponentTypeID { get; set; }
        public List<FilterCondition>? Filters { get; set; }
        public FilterLogic FilterLogic { get; set; } = FilterLogic.And;
    }
}
