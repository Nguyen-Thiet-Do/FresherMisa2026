using System;

namespace FresherMisa2026.Entities.Candidate.DTO
{
    public class CandidateFilterRequest
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
        public string? Level { get; set; }
        public string? City { get; set; }
        public string? JobPosition { get; set; }
        public string? Department { get; set; }
        public string? CandidateSource { get; set; }
        public bool? IsEmployee { get; set; }
        public DateTime? HiringDateFrom { get; set; }
        public DateTime? HiringDateTo { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageIndex { get; set; } = 1;
    }
}
