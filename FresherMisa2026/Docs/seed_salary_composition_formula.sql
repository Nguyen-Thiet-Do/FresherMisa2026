-- Cập nhật công thức cho pa_salary_composition
-- Quy ước: [CODE] = tham chiếu thành phần lương khác
-- Lương cơ sở 2024: 2,340,000 VND → mức trần BHXH/BHYT = 20 × 2,340,000 = 46,800,000
-- Lương tối thiểu vùng I 2024: 4,960,000 VND → mức trần BHTN = 20 × 4,960,000

SET NAMES utf8mb4;
USE amis_tien_luong;

-- ===========================================================
-- KẾ THỪA HỆ THỐNG (Source=2)
-- ===========================================================

-- LUONG_CO_BAN: nhập tay, không có công thức
-- NormFormula = mức lương tối đa tính BHXH/BHYT (20 × lương cơ sở)
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = NULL,
    NormFormula  = '46800000'
WHERE Code = 'LUONG_CO_BAN';

-- LUONG_DONG_BH: bằng lương cơ bản (đơn giản hóa — thực tế có thể khác)
-- NormFormula = trần đóng BHXH
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[LUONG_CO_BAN]',
    NormFormula  = '46800000'
WHERE Code = 'LUONG_DONG_BH';

-- BHXH_NLD: 8% lương đóng BH, trần 8% × 46,800,000 = 3,744,000
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[LUONG_DONG_BH] * 0.08',
    NormFormula  = '46800000 * 0.08'
WHERE Code = 'BHXH_NLD';

-- BHYT_NLD: 1.5% lương đóng BH, trần 1.5% × 46,800,000 = 702,000
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[LUONG_DONG_BH] * 0.015',
    NormFormula  = '46800000 * 0.015'
WHERE Code = 'BHYT_NLD';

-- BHTN_NLD: 1% lương đóng BH, trần 1% × 20 × lương tối thiểu vùng I
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[LUONG_DONG_BH] * 0.01',
    NormFormula  = '20 * 4960000 * 0.01'
WHERE Code = 'BHTN_NLD';

-- THUE_TNCN: phương pháp tính nhanh bậc 1 (≤60tr thu nhập tính thuế → 5%)
-- TN tính thuế = TONG_THU_NHAP - BHXH - BHYT - BHTN - giảm trừ bản thân 11tr - giảm trừ người phụ thuộc 4.4tr/người
-- Đây là công thức minh hoạ bậc 1; thực tế cần hàm luỹ tiến
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = 'MAX(([TONG_THU_NHAP] - [BHXH_NLD] - [BHYT_NLD] - [BHTN_NLD] - 11000000 - [SO_NGUOI_PHU_THUOC] * 4400000) * 0.1, 0)',
    NormFormula  = NULL
WHERE Code = 'THUE_TNCN';

-- SO_CONG_CHUAN: mặc định 26 ngày/tháng (nhập tay hoặc lấy từ hệ thống chấm công)
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = '26',
    NormFormula  = '31'
WHERE Code = 'SO_CONG_CHUAN';

-- SO_CONG_THUC_TE: nhập tay từ chấm công, không vượt quá số công chuẩn
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = NULL,
    NormFormula  = '[SO_CONG_CHUAN]'
WHERE Code = 'SO_CONG_THUC_TE';

-- TONG_THU_NHAP: tự động cộng tổng tất cả khoản Nature=1
-- ValueMode=1 (AutoSum) — không cần ValueFormula
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = NULL,
    NormFormula  = NULL
WHERE Code = 'TONG_THU_NHAP';

-- THUC_LINH = Tổng thu nhập - tổng khấu trừ (BHXH+BHYT+BHTN+TNCN+CĐ+phạt)
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[TONG_THU_NHAP] - [BHXH_NLD] - [BHYT_NLD] - [BHTN_NLD] - [THUE_TNCN] - [CONG_DOAN_NV] - [PHAT_NGHI_KHONG_PHEP]',
    NormFormula  = NULL
WHERE Code = 'THUC_LINH';

-- ===========================================================
-- ĐƠN VỊ TỰ TẠO (Source=1)
-- ===========================================================

-- LUONG_KINH_DOANH: lương tính theo ngày công thực tế
-- = Lương cơ bản × (Công thực tế / Công chuẩn)
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[LUONG_CO_BAN] * ([SO_CONG_THUC_TE] / [SO_CONG_CHUAN])',
    NormFormula  = '[LUONG_CO_BAN]'
WHERE Code = 'LUONG_KINH_DOANH';

-- PHU_CAP_AN_TRUA: cố định, NormFormula = mức tối đa miễn thuế theo TT96/2015
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = '730000',
    NormFormula  = '730000'
WHERE Code = 'PHU_CAP_AN_TRUA';

-- PHU_CAP_DIEN_THOAI: cố định theo chính sách công ty
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = '500000',
    NormFormula  = '1000000'
WHERE Code = 'PHU_CAP_DIEN_THOAI';

-- PHU_CAP_XANG_XE: cố định, NormFormula = mức tối đa miễn thuế
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = '1000000',
    NormFormula  = '1500000'
WHERE Code = 'PHU_CAP_XANG_XE';

-- PHU_CAP_NHA_O: cố định theo chính sách nội bộ
-- NormFormula = không quá 15% tổng thu nhập (quy định nội bộ)
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = '2000000',
    NormFormula  = '[TONG_THU_NHAP] * 0.15'
WHERE Code = 'PHU_CAP_NHA_O';

-- PHU_CAP_TRANG_PHUC: ngừng theo dõi, giữ nguyên NormFormula = 5,000,000/năm
UPDATE pa_salary_composition SET
    ValueMode    = 1,
    ValueFormula = '416000',
    NormFormula  = '5000000 / 12'
WHERE Code = 'PHU_CAP_TRANG_PHUC';

-- THUONG_HIEU_QUA: thưởng hiệu quả công việc = 30% lương cơ bản × hệ số đánh giá
-- Hệ số đánh giá nhập ngoài (0–2), trần = 50% lương cơ bản
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[LUONG_CO_BAN] * 0.3 * [HE_SO_DANH_GIA]',
    NormFormula  = '[LUONG_CO_BAN] * 0.5'
WHERE Code = 'THUONG_HIEU_QUA';

-- HOA_HONG: 2% doanh số thực thu trong tháng, không có trần (AllowExceedNorm=0 → tự động giới hạn)
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[DOANH_SO_THANG] * 0.02',
    NormFormula  = '[DOANH_SO_THANG] * 0.05',
    AllowExceedNorm = 1
WHERE Code = 'HOA_HONG';

-- PHAT_NGHI_KHONG_PHEP: = (Lương cơ bản / Công chuẩn) × Số ngày vắng không phép
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '([LUONG_CO_BAN] / [SO_CONG_CHUAN]) * [SO_NGAY_VANG_KHONG_PHEP]',
    NormFormula  = '[LUONG_CO_BAN]'
WHERE Code = 'PHAT_NGHI_KHONG_PHEP';

-- CONG_DOAN_NV: 1% tổng thu nhập, trần = 1% lương cơ bản (quy định nội bộ)
UPDATE pa_salary_composition SET
    ValueMode    = 2,
    ValueFormula = '[TONG_THU_NHAP] * 0.01',
    NormFormula  = '[LUONG_CO_BAN] * 0.01'
WHERE Code = 'CONG_DOAN_NV';
