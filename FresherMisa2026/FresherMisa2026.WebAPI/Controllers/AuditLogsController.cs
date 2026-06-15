using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogsController(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        /// <summary>
        /// Lấy lịch sử thao tác của một bản ghi cụ thể.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ServiceResponse>> GetAuditLogs(
            [FromQuery] string? entityType,
            [FromQuery] Guid? entityId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize  = 20)
        {
            if (entityType != null && entityId.HasValue)
            {
                var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId.Value);
                return Ok(new ServiceResponse { IsSuccess = true, Code = (int)ResponseCode.Success, Data = logs });
            }

            var paged = await _auditLogRepository.GetPagedAsync(entityType, pageIndex, pageSize);
            return Ok(new ServiceResponse { IsSuccess = true, Code = (int)ResponseCode.Success, Data = paged });
        }
    }
}
