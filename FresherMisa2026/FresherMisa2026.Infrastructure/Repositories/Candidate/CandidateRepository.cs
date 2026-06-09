using Dapper;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Candidate;
using FresherMisa2026.Entities.Candidate.DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class CandidateRepository : BaseRepository<Candidate>, ICandidateRepository
    {
        public CandidateRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<Candidate>> logger)
            : base(configuration, cache, logger)
        {
        }

        public async Task<PagingResponse<Candidate>> FilterCandidatesPagingAsync(CandidateFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@v_Search", request.Search);
            parameters.Add("@v_Gender", request.Gender);
            parameters.Add("@v_Level", request.Level);
            parameters.Add("@v_City", request.City);
            parameters.Add("@v_JobPosition", request.JobPosition);
            parameters.Add("@v_Department", request.Department);
            parameters.Add("@v_CandidateSource", request.CandidateSource);
            parameters.Add("@v_IsEmployee", request.IsEmployee);
            parameters.Add("@v_HiringDateFrom", request.HiringDateFrom);
            parameters.Add("@v_HiringDateTo", request.HiringDateTo);
            parameters.Add("@v_PageSize", request.PageSize);
            parameters.Add("@v_PageIndex", request.PageIndex);
            parameters.Add("@v_Total", dbType: DbType.Int64, direction: ParameterDirection.Output);

            using var connection = CreateConnection();
            var data = await connection.QueryAsync<Candidate>(
                "Proc_Candidate_Filter_Paging",
                parameters,
                commandType: CommandType.StoredProcedure);

            var total = parameters.Get<long>("@v_Total");

            return new PagingResponse<Candidate>
            {
                Total = total,
                Data = data.ToList()
            };
        }

        public async Task<Candidate?> GetByPhoneNumberAsync(string phoneNumber)
        {
            string query = SQLExtension.GetQuery("Candidate.GetByPhoneNumber");
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Candidate>(query, new { PhoneNumber = phoneNumber }, commandType: CommandType.Text);
        }

        public async Task<Candidate?> GetByEmailAsync(string email)
        {
            string query = SQLExtension.GetQuery("Candidate.GetByEmail");
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Candidate>(query, new { Email = email }, commandType: CommandType.Text);
        }
    }
}
