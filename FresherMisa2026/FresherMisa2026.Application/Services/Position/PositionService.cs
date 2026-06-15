using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Position;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FresherMisa2026.Application.Services
{
    public class PositionService : BaseService<Position>, IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public PositionService(
            IBaseRepository<Position> baseRepository,
            IPositionRepository positionRepository,
            IEmployeeRepository employeeRepository,
            IAuditLogRepository auditLogRepository
            ) : base(baseRepository, auditLogRepository)
        {
            _positionRepository = positionRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<ServiceResponse> GetPositionByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Code = 400,
                    UserMessage = "Mã chức vụ không được để trống",
                    DevMessage = "Mã chức vụ không được để trống"
                };
            }

            var position = await _positionRepository.GetPositionByCode(code);
            if (position == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Code = 404,
                    UserMessage = "Không tìm thấy chức vụ",
                    DevMessage = "Không tìm thấy chức vụ"
                };
            }

            return new ServiceResponse
            {
                IsSuccess = true,
                Code = 200,
                Data = position
            };
        }

        protected override async Task<bool> ValidateBeforeDeleteAsync(Guid entityId, Position existingEntity)
        {
            var employees = await _employeeRepository.GetEmployeesByPositionId(entityId);
            return !employees.Any();
        }

        protected override Task<string?> GetDeleteValidationMessageAsync(Guid entityId, Position existingEntity)
        {
            return Task.FromResult<string?>("Không thể xóa chức vụ vì đang có nhân viên thuộc chức vụ này");
        }

        protected override async Task<List<ValidationError>> ValidateBeforeInsertAsync(Position position)
        {
            var errors = new List<ValidationError>();
            var duplicateError = await ValidateDuplicateCodeAsync(position.PositionCode, null);
            if (duplicateError != null)
            {
                errors.Add(duplicateError);
            }

            return errors;
        }

        protected override async Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, Position position, Position existingEntity)
        {
            var errors = new List<ValidationError>();
            var duplicateError = await ValidateDuplicateCodeAsync(position.PositionCode, entityId);
            if (duplicateError != null)
            {
                errors.Add(duplicateError);
            }

            return errors;
        }

        protected override List<ValidationError> ValidateCustom(Position position)
        {
            var errors = new List<ValidationError>();

            if (!string.IsNullOrEmpty(position.PositionCode) && position.PositionCode.Length > 20)
            {
                errors.Add(new ValidationError("PositionCode", "Mã vị trí không được vượt quá 20 ký tự"));
            }

            if (!string.IsNullOrEmpty(position.PositionName) && position.PositionName.Length > 255)
            {
                errors.Add(new ValidationError("PositionName", "Tên vị trí không được vượt quá 255 ký tự"));
            }

            return errors;
        }

        /// Created By: ntdo (2026-04-22)
        private async Task<ValidationError?> ValidateDuplicateCodeAsync(string? positionCode, Guid? currentPositionId)
        {
            if (string.IsNullOrWhiteSpace(positionCode))
            {
                return null;
            }

            var existingPosition = await _positionRepository.GetPositionByCode(positionCode.Trim());
            if (existingPosition == null)
            {
                return null;
            }

            if (!currentPositionId.HasValue || existingPosition.PositionID != currentPositionId.Value)
            {
                return new ValidationError(nameof(Position.PositionCode), "PositionCode đã tồn tại");
            }

            return null;
        }
    }
}