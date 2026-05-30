-- Seed data cho pa_salary_composition
-- 10 bản ghi Source=2 (kế thừa hệ thống) + 10 bản ghi Source=1 (đơn vị tự tạo)
SET NAMES utf8mb4;
USE amis_tien_luong;

TRUNCATE TABLE pa_salary_composition;

INSERT INTO pa_salary_composition (
  SalaryCompositionID, Code, Name, OrganizationID, ComponentTypeID,
  SystemCompositionID, Nature, TaxType, ValueType, ValueMode,
  ValueFormula, ValueScope, NormFormula, AllowExceedNorm, Description,
  ShowOnPayslip, HideWhenZero, Source, Status, IsDeleted,
  CreatedBy, CreateDate
) VALUES
-- ===== Source=2: KẾ THỪA HỆ THỐNG (10 bản ghi) =====
('00000000-0000-0000-0003-000000000001', 'LUONG_CO_BAN',       'Lương cơ bản',               NULL, '00000000-0000-0000-0000-000000000006', '00000000-0000-0000-0002-000000000001', 1, 1,    1, 1, NULL, NULL, NULL, 0, 'Lương cơ bản theo hợp đồng',                 1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000002', 'LUONG_DONG_BH',      'Lương đóng bảo hiểm',        NULL, '00000000-0000-0000-0000-000000000008', '00000000-0000-0000-0002-000000000002', 1, 1,    1, 1, NULL, NULL, NULL, 0, 'Lương làm căn cứ đóng bảo hiểm',             1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000008', 'BHXH_NLD',           'BHXH người lao động (8%)',   NULL, '00000000-0000-0000-0000-000000000008', '00000000-0000-0000-0002-000000000008', 2, NULL, 1, 2, NULL, NULL, NULL, 0, 'Bảo hiểm xã hội NLĐ đóng 8% lương đóng BH', 1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000009', 'BHYT_NLD',           'BHYT người lao động (1.5%)', NULL, '00000000-0000-0000-0000-000000000008', '00000000-0000-0000-0002-000000000009', 2, NULL, 1, 2, NULL, NULL, NULL, 0, 'Bảo hiểm y tế NLĐ đóng 1.5% lương đóng BH', 1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000010', 'BHTN_NLD',           'BHTN người lao động (1%)',   NULL, '00000000-0000-0000-0000-000000000008', '00000000-0000-0000-0002-000000000010', 2, NULL, 1, 2, NULL, NULL, NULL, 0, 'Bảo hiểm thất nghiệp NLĐ đóng 1%',          1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000011', 'THUE_TNCN',          'Thuế thu nhập cá nhân',      NULL, '00000000-0000-0000-0000-000000000007', '00000000-0000-0000-0002-000000000011', 2, NULL, 1, 2, NULL, NULL, NULL, 0, 'Thuế TNCN khấu trừ tại nguồn',               1, 1, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000012', 'SO_CONG_CHUAN',      'Số công chuẩn',              NULL, '00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0002-000000000012', 3, NULL, 2, 1, NULL, NULL, NULL, 0, 'Số ngày công chuẩn trong tháng',             1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000013', 'SO_CONG_THUC_TE',    'Số công thực tế',            NULL, '00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0002-000000000013', 3, NULL, 2, 1, NULL, NULL, NULL, 0, 'Số ngày công thực tế nhân viên đã làm',      1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000015', 'TONG_THU_NHAP',      'Tổng thu nhập',              NULL, '00000000-0000-0000-0000-000000000006', '00000000-0000-0000-0002-000000000015', 4, NULL, 1, 1, NULL, NULL, NULL, 0, 'Tổng thu nhập trước khấu trừ',               1, 0, 2, 1, 0, 'system', NOW()),
('00000000-0000-0000-0003-000000000019', 'THUC_LINH',          'Thực lĩnh',                  NULL, '00000000-0000-0000-0000-000000000006', '00000000-0000-0000-0002-000000000019', 4, NULL, 1, 2, NULL, NULL, NULL, 0, 'Số tiền thực tế nhân viên được nhận',        1, 0, 2, 1, 0, 'system', NOW()),

-- ===== Source=1: ĐƠN VỊ TỰ TẠO (10 bản ghi) =====
('00000000-0000-0000-0003-000000000101', 'LUONG_KINH_DOANH',   'Lương kinh doanh',           NULL, '00000000-0000-0000-0000-000000000006', NULL, 1, 1,    1, 2, NULL, NULL, NULL, 0, 'Lương theo doanh số đạt được trong tháng',   1, 1, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000102', 'PHU_CAP_AN_TRUA',    'Phụ cấp tiền ăn trưa',       NULL, '00000000-0000-0000-0000-000000000006', NULL, 1, 2,    1, 1, NULL, NULL, NULL, 0, 'Phụ cấp tiền ăn trưa, mức tối đa 730k/tháng miễn thuế', 1, 0, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000103', 'PHU_CAP_DIEN_THOAI', 'Phụ cấp điện thoại',         NULL, '00000000-0000-0000-0000-000000000006', NULL, 1, 2,    1, 1, NULL, NULL, NULL, 0, 'Phụ cấp tiền điện thoại hàng tháng',         1, 1, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000104', 'PHU_CAP_XANG_XE',    'Phụ cấp xăng xe',            NULL, '00000000-0000-0000-0000-000000000006', NULL, 1, 2,    1, 1, NULL, NULL, NULL, 0, 'Phụ cấp đi lại, xăng xe hàng tháng',        1, 1, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000105', 'PHU_CAP_NHA_O',      'Phụ cấp nhà ở',              NULL, '00000000-0000-0000-0000-000000000006', NULL, 1, 1,    1, 1, NULL, NULL, NULL, 0, 'Hỗ trợ tiền thuê nhà ở cho nhân viên',       1, 1, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000106', 'THUONG_HIEU_QUA',    'Thưởng hiệu quả công việc',  NULL, '00000000-0000-0000-0000-000000000004', NULL, 1, 1,    1, 2, NULL, NULL, NULL, 0, 'Thưởng hàng quý dựa trên đánh giá hiệu quả', 1, 1, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000107', 'HOA_HONG',           'Hoa hồng bán hàng',          NULL, '00000000-0000-0000-0000-000000000003', NULL, 1, 1,    1, 2, NULL, NULL, NULL, 0, 'Hoa hồng tính theo % doanh số bán hàng',     1, 1, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000108', 'PHAT_NGHI_KHONG_PHEP','Phạt nghỉ không phép',      NULL, '00000000-0000-0000-0000-000000000006', NULL, 2, NULL, 1, 2, NULL, NULL, NULL, 0, 'Khấu trừ lương những ngày nghỉ không có phép', 1, 1, 1, 1, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000109', 'PHU_CAP_TRANG_PHUC', 'Phụ cấp trang phục',         NULL, '00000000-0000-0000-0000-000000000006', NULL, 1, 2,    1, 1, NULL, NULL, NULL, 0, 'Hỗ trợ chi phí trang phục làm việc',         1, 1, 1, 0, 0, 'admin', NOW()),
('00000000-0000-0000-0003-000000000110', 'CONG_DOAN_NV',       'Đoàn phí công đoàn',         NULL, '00000000-0000-0000-0000-000000000008', NULL, 2, NULL, 3, 2, NULL, NULL, NULL, 0, 'Đoàn phí công đoàn theo quy định nội bộ',   1, 1, 1, 1, 0, 'admin', NOW());
