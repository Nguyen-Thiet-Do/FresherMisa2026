namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    public class ExitDataRequest
    {
        public List<Guid> Ids { get; set; } = new();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
