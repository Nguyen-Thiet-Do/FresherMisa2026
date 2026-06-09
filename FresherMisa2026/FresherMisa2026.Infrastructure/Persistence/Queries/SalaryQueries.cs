namespace FresherMisa2026.Infrastructure.Persistence.Queries
{
    /// <summary>
    /// Tập trung toàn bộ raw SQL của module Tiền Lương.
    /// Đặt SQL ngoài Repository để dễ review, test và tránh duplicate fragment.
    /// </summary>
    /// <remarks>Created by: ntdo — 03/06/2026 · snake_case: 05/06/2026</remarks>
    public static class SalaryQueries
    {
        // ─── pa_salary_composition ───────────────────────────────────────────

        /// <summary>Subquery lấy danh sách OrganizationID cách nhau bằng ',' cho 1 TPL.</summary>
        public const string OrganizationIdsSubquery = @"
            (SELECT GROUP_CONCAT(sco.`organization_id` ORDER BY sco.`organization_id` SEPARATOR ',')
             FROM pa_salary_composition_organization sco
             WHERE sco.`salary_composition_id` = sc.`salary_composition_id`) AS organization_ids";

        /// <summary>Subquery lấy tên các đơn vị áp dụng (đã JOIN pa_organization) cách nhau bằng ', '.</summary>
        public const string OrganizationNamesSubquery = @"
            (SELECT GROUP_CONCAT(org2.`name` ORDER BY sco2.`organization_id` SEPARATOR ', ')
             FROM pa_salary_composition_organization sco2
             JOIN pa_organization org2 ON sco2.`organization_id` = org2.`organization_id`
             WHERE sco2.`salary_composition_id` = sc.`salary_composition_id`) AS organization_names";

        /// <summary>SELECT base của SalaryComposition kèm ComponentTypeName + 2 subquery tổ chức.</summary>
        public static readonly string SelectSalaryCompositionWithJoin = $@"
            SELECT sc.*, ct.`name` AS component_type_name,
                   {OrganizationIdsSubquery},
                   {OrganizationNamesSubquery}
            FROM   pa_salary_composition sc
            LEFT JOIN pa_salary_component_type ct ON sc.`component_type_id` = ct.`component_type_id`";

        /// <summary>Insert một dòng vào bảng junction TPL ↔ Đơn vị.</summary>
        public const string InsertSalaryCompositionOrganization = @"
            INSERT INTO pa_salary_composition_organization (`salary_composition_id`, `organization_id`)
            VALUES (@ScId, @OrgId)";

        /// <summary>Xóa toàn bộ liên kết Đơn vị của một TPL (dùng trước khi insert lại danh sách mới).</summary>
        public const string DeleteSalaryCompositionOrganizations = @"
            DELETE FROM pa_salary_composition_organization
            WHERE `salary_composition_id` = @ScId";

        // ─── pa_salary_composition_system ────────────────────────────────────

        /// <summary>SELECT base của SalaryCompositionSystem kèm ComponentTypeName.</summary>
        public const string SelectSalaryCompositionSystemWithJoin = @"
            SELECT sc.*, ct.`name` AS component_type_name
            FROM   pa_salary_composition_system sc
            LEFT JOIN pa_salary_component_type ct ON sc.`component_type_id` = ct.`component_type_id`";

        /// <summary>
        /// Subquery loại trừ các TPL hệ thống đã được kế thừa bởi đơn vị
        /// (Source = InheritedFromSystem trong pa_salary_composition).
        /// {0} = giá trị của enum SalaryCompositionSource.InheritedFromSystem.
        /// </summary>
        public const string ExcludeInheritedSystemCompositionsTemplate =
            @"`system_composition_id` NOT IN (
                SELECT `system_composition_id`
                FROM   pa_salary_composition
                WHERE  `source` = {0} AND `is_deleted` = FALSE)";

        // ─── pa_grid_config ──────────────────────────────────────────────────

        /// <summary>Xóa toàn bộ cấu hình lưới của 1 user cho 1 grid (dùng cho reset hoặc upsert).</summary>
        public const string DeleteGridConfigByUserAndGrid = @"
            DELETE FROM pa_grid_config
            WHERE `user_id` = @userID AND `grid_code` = @gridCode";

        /// <summary>Insert một dòng cấu hình cột lưới — dùng cùng transaction sau khi DELETE.</summary>
        public const string InsertGridConfig = @"
            INSERT INTO pa_grid_config
              (`grid_config_id`, `user_id`, `grid_code`, `column_key`, `caption`, `order_index`, `width`,
               `is_pinned`, `pin_position`, `is_visible`, `created_by`, `create_date`)
            VALUES
              (@GridConfigID, @UserID, @GridCode, @ColumnKey, @Caption, @OrderIndex, @Width,
               @IsPinned, @PinPosition, @IsVisible, @CreatedBy, @CreateDate)";

        // ─── Stored Procedure names (snake_case: proc_{table}_{action}) ─────

        public const string ProcSalaryCompositionFilter           = "proc_pa_salary_composition_filter";
        public const string ProcSalaryCompositionAdvancedFilter   = "proc_pa_salary_composition_advanced_filter_paging";
        public const string ProcSalaryCompositionSystemFilter     = "proc_pa_salary_composition_system_filter";
        public const string ProcSalaryCompositionSystemAdvanced   = "proc_pa_salary_composition_system_advanced_filter_paging";
        public const string ProcGridConfigGetByGrid               = "proc_pa_grid_config_get_by_grid";
    }
}
