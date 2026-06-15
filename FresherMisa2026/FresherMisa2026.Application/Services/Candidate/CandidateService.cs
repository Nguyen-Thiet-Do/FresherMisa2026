using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Candidate;
using FresherMisa2026.Entities.Candidate.DTO;
using FresherMisa2026.Entities.Enums;
using System.Text.RegularExpressions;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho Candidate
    /// Created By: ntdo (2026-04-24)
    /// </summary>
    public class CandidateService : BaseService<Candidate>, ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IFileService _fileService;
        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PhoneNumberRegex = new(@"^(?:0|\+84)(?:3|5|7|8|9)\d{8}$", RegexOptions.Compiled);
        public CandidateService(
            IBaseRepository<Candidate> baseRepository,
            ICandidateRepository candidateRepository,
            IFileService fileService,
            IAuditLogRepository auditLogRepository) : base(baseRepository, auditLogRepository)
        {
            _candidateRepository = candidateRepository;
            _fileService = fileService;
        }

        /// <summary>Validate trước khi thêm ứng viên</summary>
        /// Created By: ntdo (2026-04-24)
        protected override async Task<List<ValidationError>> ValidateBeforeInsertAsync(Candidate candidate)
        {
            return await ValidateDuplicateAsync(candidate, null);
        }

        /// <summary>Validate trước khi cập nhật ứng viên</summary>
        /// Created By: ntdo (2026-04-24)
        protected override async Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, Candidate candidate, Candidate existingEntity)
        {
            return await ValidateDuplicateAsync(candidate, entityId);
        }

        /// <summary>Kiểm tra trùng số điện thoại và email ứng viên</summary>
        /// Created By: ntdo (2026-04-25)
        private async Task<List<ValidationError>> ValidateDuplicateAsync(Candidate candidate, Guid? currentId)
        {
            var errors = new List<ValidationError>();

            if (!string.IsNullOrWhiteSpace(candidate.PhoneNumber))
            {
                var existing = await _candidateRepository.GetByPhoneNumberAsync(candidate.PhoneNumber.Trim());
                if (existing != null && (!currentId.HasValue || existing.CandidateID != currentId.Value))
                    errors.Add(new ValidationError(nameof(Candidate.PhoneNumber), "Số điện thoại đã tồn tại"));
            }

            if (!string.IsNullOrWhiteSpace(candidate.Email))
            {
                var existing = await _candidateRepository.GetByEmailAsync(candidate.Email.Trim());
                if (existing != null && (!currentId.HasValue || existing.CandidateID != currentId.Value))
                    errors.Add(new ValidationError(nameof(Candidate.Email), "Email đã tồn tại"));
            }

            return errors;
        }

        /// <summary>Validate tùy chỉnh cho Candidate</summary>
        /// Created By: ntdo (2026-04-25)
        protected override List<ValidationError> ValidateCustom(Candidate candidate)
        {
            var errors = new List<ValidationError>();

            if (candidate.DateOfBirth.HasValue && candidate.DateOfBirth.Value > DateTime.Now)
                errors.Add(new ValidationError(nameof(Candidate.DateOfBirth), "Ngày sinh không được lớn hơn ngày hiện tại"));

            if (!string.IsNullOrWhiteSpace(candidate.Email) && !EmailRegex.IsMatch(candidate.Email.Trim()))
                errors.Add(new ValidationError(nameof(Candidate.Email), "Email không đúng định dạng"));

            if (!string.IsNullOrWhiteSpace(candidate.PhoneNumber) && !PhoneNumberRegex.IsMatch(candidate.PhoneNumber.Trim()))
                errors.Add(new ValidationError(nameof(Candidate.PhoneNumber), "Số điện thoại không đúng định dạng"));

            if (candidate.WorkStartDate.HasValue && candidate.WorkEndDate.HasValue && candidate.WorkStartDate > candidate.WorkEndDate)
                errors.Add(new ValidationError(nameof(Candidate.WorkEndDate), "Ngày kết thúc phải sau ngày bắt đầu làm việc"));

            return errors;
        }

        /// <summary>Lọc ứng viên có phân trang</summary>
        /// Created By: ntdo (2026-04-26)
        public async Task<ServiceResponse> FilterCandidatesPagingAsync(CandidateFilterRequest request)
        {
            var validationError = ValidateFilterRequest(request);
            if (validationError != null) return validationError;

            if (request.PageSize <= 0) request.PageSize = 10;
            if (request.PageIndex <= 0) request.PageIndex = 1;

            var pagingResult = await _candidateRepository.FilterCandidatesPagingAsync(request);

            return CreateSuccessResponse(new
            {
                pagingResult.Total,
                PageSize = request.PageSize,
                PageIndex = request.PageIndex,
                pagingResult.Data
            });
        }

        /// <summary>Sau khi xóa ứng viên: xóa file CV và avatar</summary>
        /// Created By: ntdo (2026-04-27)
        protected override void AfterDelete(Candidate candidate)
        {
            _fileService.DeleteFile(candidate.CVFile);
            _fileService.DeleteFile(candidate.Avatar);
        }

        /// <summary>Validate các tham số filter ứng viên</summary>
        /// Created By: ntdo (2026-04-26)
        private ServiceResponse? ValidateFilterRequest(CandidateFilterRequest request)
        {
            if (request.HiringDateFrom.HasValue && request.HiringDateTo.HasValue && request.HiringDateFrom > request.HiringDateTo)
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "hiringDateFrom không được lớn hơn hiringDateTo",
                    "hiringDateFrom không được lớn hơn hiringDateTo");

            return null;
        }
    }
}
