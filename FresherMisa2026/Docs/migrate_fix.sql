-- ============================================================
-- MIGRATION: Fix UUID + Ten tieng Viet
-- Chay sau khi da import thanh_phan_luong_amis.sql
-- ============================================================

USE amis_tien_luong;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- BUOC 1: Cap nhat UUID cac bang lookup truoc
--         (khong co FK phu thuoc vao chung tu cac bang khac)
-- ============================================================

-- ---- 1a. pa_salary_component_type ----
UPDATE pa_salary_component_type SET component_type_id = '00000000-0000-0000-0000-000000000001', name = 'Lương'         WHERE code = 'LUONG';
UPDATE pa_salary_component_type SET component_type_id = '00000000-0000-0000-0000-000000000002', name = 'Phụ cấp'       WHERE code = 'PHU_CAP';
UPDATE pa_salary_component_type SET component_type_id = '00000000-0000-0000-0000-000000000003', name = 'Bảo hiểm'      WHERE code = 'BAO_HIEM';
UPDATE pa_salary_component_type SET component_type_id = '00000000-0000-0000-0000-000000000004', name = 'Khấu trừ'      WHERE code = 'KHAU_TRU';
UPDATE pa_salary_component_type SET component_type_id = '00000000-0000-0000-0000-000000000005', name = 'Chấm công'     WHERE code = 'CHAM_CONG';
UPDATE pa_salary_component_type SET component_type_id = '00000000-0000-0000-0000-000000000006', name = 'Thuế TNCN'     WHERE code = 'THUE_TNCN';
UPDATE pa_salary_component_type SET component_type_id = '00000000-0000-0000-0000-000000000007', name = 'Khác'          WHERE code = 'KHAC';

-- ---- 1b. pa_organization ----
UPDATE pa_organization SET organization_id = '00000000-0000-0000-0001-000000000001', name = 'Công ty TNHH MISA',            parent_id = NULL                                   WHERE code = 'CTY';
UPDATE pa_organization SET organization_id = '00000000-0000-0000-0001-000000000002', name = 'Phòng Kinh doanh',              parent_id = '00000000-0000-0000-0001-000000000001' WHERE code = 'KD';
UPDATE pa_organization SET organization_id = '00000000-0000-0000-0001-000000000003', name = 'Phòng Kế toán',                 parent_id = '00000000-0000-0000-0001-000000000001' WHERE code = 'KT';
UPDATE pa_organization SET organization_id = '00000000-0000-0000-0001-000000000004', name = 'Phòng CNTT',                    parent_id = '00000000-0000-0000-0001-000000000001' WHERE code = 'CNTT';
UPDATE pa_organization SET organization_id = '00000000-0000-0000-0001-000000000005', name = 'Phòng Nhân sự',                 parent_id = '00000000-0000-0000-0001-000000000001' WHERE code = 'NS';

-- ============================================================
-- BUOC 2: Cap nhat FK trong pa_salary_composition_system
--         (component_type_id phai khop voi UUID moi o buoc 1a)
-- ============================================================
UPDATE pa_salary_composition_system SET component_type_id = '00000000-0000-0000-0000-000000000001' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000001';
UPDATE pa_salary_composition_system SET component_type_id = '00000000-0000-0000-0000-000000000002' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000002';
UPDATE pa_salary_composition_system SET component_type_id = '00000000-0000-0000-0000-000000000003' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000003';
UPDATE pa_salary_composition_system SET component_type_id = '00000000-0000-0000-0000-000000000004' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000004';
UPDATE pa_salary_composition_system SET component_type_id = '00000000-0000-0000-0000-000000000005' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000005';
UPDATE pa_salary_composition_system SET component_type_id = '00000000-0000-0000-0000-000000000006' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000006';
UPDATE pa_salary_composition_system SET component_type_id = '00000000-0000-0000-0000-000000000007' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000007';

-- Cap nhat PK + ten tieng Viet cho pa_salary_composition_system
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000001', name = 'Lương cơ bản'                  WHERE code = 'LUONG_CO_BAN';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000002', name = 'Lương đóng bảo hiểm'            WHERE code = 'LUONG_DONG_BH';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000003', name = 'Phụ cấp chức vụ'                WHERE code = 'PHU_CAP_CHUC_VU';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000004', name = 'Phụ cấp điện thoại'             WHERE code = 'PHU_CAP_DIEN_THOAI';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000005', name = 'Phụ cấp tiền ăn trưa'           WHERE code = 'PHU_CAP_AN_TRUA';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000006', name = 'Phụ cấp xăng xe'                WHERE code = 'PHU_CAP_XANG_XE';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000007', name = 'Phụ cấp công tác phí'           WHERE code = 'PHU_CAP_CONG_TAC';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000008', name = 'BHXH người lao động (8%)'        WHERE code = 'BHXH_NLD';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000009', name = 'BHYT người lao động (1.5%)'      WHERE code = 'BHYT_NLD';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000010', name = 'BHTN người lao động (1%)'        WHERE code = 'BHTN_NLD';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000011', name = 'Thuế thu nhập cá nhân'           WHERE code = 'THUE_TNCN';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000012', name = 'Số công chuẩn'                   WHERE code = 'SO_CONG_CHUAN';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000013', name = 'Số công thực tế'                 WHERE code = 'SO_CONG_THUC_TE';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000014', name = 'Số người phụ thuộc'              WHERE code = 'SO_NGUOI_PHU_THUOC';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000015', name = 'Tổng thu nhập'                   WHERE code = 'TONG_THU_NHAP';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000016', name = 'Tổng khấu trừ'                   WHERE code = 'TONG_KHAU_TRU';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000017', name = 'Lương kỳ này'                    WHERE code = 'LUONG_KY_NAY';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000018', name = 'Tạm ứng'                         WHERE code = 'TAM_UNG';
UPDATE pa_salary_composition_system SET system_composition_id = '00000000-0000-0000-0002-000000000019', name = 'Thực lĩnh'                       WHERE code = 'THUC_LINH';

-- ============================================================
-- BUOC 3: Cap nhat FK trong pa_salary_composition
--         (phai lam truoc khi doi PK cua cac bang cha)
-- ============================================================

-- 3a. organization_id
UPDATE pa_salary_composition SET organization_id = '00000000-0000-0000-0001-000000000001' WHERE organization_id = 'org00001-0000-0000-0000-000000000001';
UPDATE pa_salary_composition SET organization_id = '00000000-0000-0000-0001-000000000002' WHERE organization_id = 'org00001-0000-0000-0000-000000000002';
UPDATE pa_salary_composition SET organization_id = '00000000-0000-0000-0001-000000000004' WHERE organization_id = 'org00001-0000-0000-0000-000000000004';

-- 3b. component_type_id
UPDATE pa_salary_composition SET component_type_id = '00000000-0000-0000-0000-000000000001' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000001';
UPDATE pa_salary_composition SET component_type_id = '00000000-0000-0000-0000-000000000002' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000002';
UPDATE pa_salary_composition SET component_type_id = '00000000-0000-0000-0000-000000000003' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000003';
UPDATE pa_salary_composition SET component_type_id = '00000000-0000-0000-0000-000000000004' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000004';
UPDATE pa_salary_composition SET component_type_id = '00000000-0000-0000-0000-000000000005' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000005';
UPDATE pa_salary_composition SET component_type_id = '00000000-0000-0000-0000-000000000006' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000006';
UPDATE pa_salary_composition SET component_type_id = '00000000-0000-0000-0000-000000000007' WHERE component_type_id = 'ctype001-0000-0000-0000-000000000007';

-- 3c. system_composition_id
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000001' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000001';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000002' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000002';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000003' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000003';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000004' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000004';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000005' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000005';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000006' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000006';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000008' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000008';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000009' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000009';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000010' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000010';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000011' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000011';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000013' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000013';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000014' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000014';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000015' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000015';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000016' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000016';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000017' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000017';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000018' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000018';
UPDATE pa_salary_composition SET system_composition_id = '00000000-0000-0000-0002-000000000019' WHERE system_composition_id = 'syscomp1-0000-0000-0000-000000000019';

-- 3d. Cap nhat PK + ten tieng Viet cho pa_salary_composition (nhom ke thua)
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000001', name = 'Lương cơ bản'                    WHERE code = 'LUONG_CO_BAN';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000002', name = 'Lương đóng bảo hiểm'              WHERE code = 'LUONG_DONG_BH';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000003', name = 'Phụ cấp chức vụ'                  WHERE code = 'PHU_CAP_CHUC_VU';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000004', name = 'Phụ cấp điện thoại'               WHERE code = 'PHU_CAP_DIEN_THOAI';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000005', name = 'Phụ cấp tiền ăn trưa'             WHERE code = 'PHU_CAP_AN_TRUA';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000006', name = 'Phụ cấp xăng xe'                  WHERE code = 'PHU_CAP_XANG_XE';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000007', name = 'BHXH người lao động (8%)'          WHERE code = 'BHXH_NLD';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000008', name = 'BHYT người lao động (1.5%)'        WHERE code = 'BHYT_NLD';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000009', name = 'BHTN người lao động (1%)'          WHERE code = 'BHTN_NLD';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000010', name = 'Thuế thu nhập cá nhân'             WHERE code = 'THUE_TNCN';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000011', name = 'Số công thực tế'                   WHERE code = 'SO_CONG_THUC_TE';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000012', name = 'Số người phụ thuộc'                WHERE code = 'SO_NGUOI_PHU_THUOC';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000013', name = 'Tổng thu nhập'                     WHERE code = 'TONG_THU_NHAP';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000014', name = 'Tổng khấu trừ'                     WHERE code = 'TONG_KHAU_TRU';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000015', name = 'Lương kỳ này'                      WHERE code = 'LUONG_KY_NAY';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000016', name = 'Tạm ứng'                           WHERE code = 'TAM_UNG';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000017', name = 'Thực lĩnh'                         WHERE code = 'THUC_LINH';

-- 3e. Cap nhat PK + ten tieng Viet cho pa_salary_composition (nhom tu them)
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000018', name = 'Lương KPI phòng Kinh doanh'        WHERE code = 'LUONG_KPI_KD';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000019', name = 'Thưởng doanh số'                   WHERE code = 'THUONG_DOANH_SO';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000020', name = 'Tỷ lệ hoàn thành KPI'              WHERE code = 'TY_LE_KPI';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000021', name = 'Lương làm thêm giờ'                WHERE code = 'LUONG_LAM_THEM';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000022', name = 'Phạt đi muộn'                      WHERE code = 'PHAT_DI_MUON';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000023', name = 'Phụ cấp năng suất cũ'              WHERE code = 'PC_NANG_SUAT_CU';
UPDATE pa_salary_composition SET salary_composition_id = '00000000-0000-0000-0003-000000000024', name = 'Phụ cấp thử nghiệm (đã xóa)'       WHERE code = 'PC_TEST_XOA_MEM';

-- ============================================================
-- BUOC 4: Cap nhat pa_grid_config
-- ============================================================
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000001', caption = 'Mã thành phần'    WHERE grid_config_id = 'gridcfg1-0000-0000-0004-000000000001';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000001', caption = 'Mã thành phần'    WHERE column_key = 'code'           AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000002', caption = 'Tên thành phần'   WHERE column_key = 'name'           AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000003', caption = 'Đơn vị áp dụng'   WHERE column_key = 'organization'   AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000004', caption = 'Loại thành phần'  WHERE column_key = 'component_type' AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000005', caption = 'Tính chất'        WHERE column_key = 'nature'         AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000006', caption = 'Kiểu giá trị'     WHERE column_key = 'value_type'     AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000007', caption = 'Giá trị'          WHERE column_key = 'value_formula'  AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000008', caption = 'Nguồn tạo'        WHERE column_key = 'source'         AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000009', caption = 'Trạng thái'       WHERE column_key = 'status'         AND user_id = 'hr_user_01';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000010', caption = 'Mã thành phần'    WHERE column_key = 'code'           AND user_id = 'hr_user_02';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000011', caption = 'Tên thành phần'   WHERE column_key = 'name'           AND user_id = 'hr_user_02';
UPDATE pa_grid_config SET grid_config_id = '00000000-0000-0000-0004-000000000012', caption = 'Giá trị'          WHERE column_key = 'value_formula'  AND user_id = 'hr_user_02';

-- ============================================================
-- KIEM TRA KET QUA
-- ============================================================
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'pa_salary_component_type' AS bang,
       component_type_id AS uuid_mau, name AS ten
FROM pa_salary_component_type ORDER BY sort_order;

SELECT 'pa_organization' AS bang,
       organization_id AS uuid_mau, name AS ten, path
FROM pa_organization ORDER BY path;

SELECT 'pa_salary_composition_system' AS bang,
       system_composition_id AS uuid_mau, code, name AS ten
FROM pa_salary_composition_system ORDER BY system_composition_id;

SELECT 'pa_salary_composition' AS bang,
       salary_composition_id AS uuid_mau, code, name AS ten, source, status, is_deleted
FROM pa_salary_composition ORDER BY salary_composition_id;

