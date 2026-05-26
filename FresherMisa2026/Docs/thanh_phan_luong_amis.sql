-- ============================================================
-- AMIS TIEN LUONG - PHAN HE THANH PHAN LUONG
-- Tai lieu: BA-SRS-TPL-01 | BA-DB-TPL-01 | De bai Fresher MISA
-- MySQL 8.x | InnoDB | utf8mb4_unicode_ci
-- Chay: mysql -u root -p < thanh_phan_luong_amis.sql
-- ============================================================

SET NAMES utf8mb4;
SET @OLD_FOREIGN_KEY_CHECKS = @@FOREIGN_KEY_CHECKS;
SET FOREIGN_KEY_CHECKS = 0;
SET @OLD_SQL_MODE = @@SQL_MODE;
SET SQL_MODE = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION';

-- ============================================================
-- DATABASE
-- ============================================================
CREATE DATABASE IF NOT EXISTS amis_tien_luong
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE amis_tien_luong;

-- ============================================================
-- XOA BANG (thu tu nguoc de tranh loi FK)
-- ============================================================
DROP TABLE IF EXISTS pa_grid_config;
DROP TABLE IF EXISTS pa_salary_composition;
DROP TABLE IF EXISTS pa_salary_composition_system;
DROP TABLE IF EXISTS pa_salary_component_type;
DROP TABLE IF EXISTS pa_organization;

-- ============================================================
-- 1. pa_organization
--    Don vi cong tac / co cau to chuc (tham chieu)
-- ============================================================
CREATE TABLE pa_organization (
  organization_id  CHAR(36)      NOT NULL,
  code             VARCHAR(50)   NOT NULL,
  name             VARCHAR(255)  NOT NULL,
  parent_id        CHAR(36)      NULL,
  path             VARCHAR(1000) NULL      COMMENT 'Materialized path VD: /CTY/KD/',
  sort_order       INT           NULL,
  is_active        TINYINT(1)    NOT NULL  DEFAULT 1,
  is_deleted       TINYINT(1)    NOT NULL  DEFAULT 0,
  created_date     DATETIME      NOT NULL  DEFAULT CURRENT_TIMESTAMP,
  created_by       VARCHAR(100)  NULL,
  modified_date    DATETIME      NULL      ON UPDATE CURRENT_TIMESTAMP,
  modified_by      VARCHAR(100)  NULL,
  PRIMARY KEY (organization_id),
  UNIQUE KEY uq_org_code (code),
  KEY idx_org_parent (parent_id),
  CONSTRAINT fk_org_parent FOREIGN KEY (parent_id)
      REFERENCES pa_organization (organization_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Don vi cong tac / co cau to chuc';

-- ============================================================
-- 2. pa_salary_component_type
--    Bang tra cuu Loai thanh phan luong
-- ============================================================
CREATE TABLE pa_salary_component_type (
  component_type_id CHAR(36)     NOT NULL,
  code             VARCHAR(50)   NOT NULL,
  name             VARCHAR(255)  NOT NULL,
  sort_order       INT           NULL,
  is_active        TINYINT(1)    NOT NULL  DEFAULT 1,
  created_date     DATETIME      NOT NULL  DEFAULT CURRENT_TIMESTAMP,
  modified_date    DATETIME      NULL      ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (component_type_id),
  UNIQUE KEY uq_ctype_code (code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Loai thanh phan luong (Luong, Phu cap, Bao hiem...)';

-- ============================================================
-- 3. pa_salary_composition_system
--    Danh muc TPL chuan cua he thong (chi doc voi nguoi dung)
-- ============================================================
CREATE TABLE pa_salary_composition_system (
  system_composition_id CHAR(36)  NOT NULL,
  code             VARCHAR(255)   NOT NULL,
  name             VARCHAR(255)   NOT NULL,
  component_type_id CHAR(36)      NULL,
  nature           TINYINT        NOT NULL  COMMENT '1=Thu nhap|2=Khau tru|3=Thong tin|4=Khac',
  tax_type         TINYINT        NULL      COMMENT '1=Chiu thue|2=Mien toan phan|3=Mien mot phan (chi khi nature=1)',
  value_type       TINYINT        NOT NULL  DEFAULT 1 COMMENT '1=Tien te|2=So|3=Phan tram',
  default_value    DECIMAL(18,4)  NULL,
  norm_formula     TEXT           NULL,
  description      VARCHAR(1000)  NULL,
  is_active        TINYINT(1)     NOT NULL  DEFAULT 1,
  created_date     DATETIME       NOT NULL  DEFAULT CURRENT_TIMESTAMP,
  modified_date    DATETIME       NULL      ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (system_composition_id),
  UNIQUE KEY uq_sys_code (code),
  KEY idx_sys_type (component_type_id),
  CONSTRAINT fk_sys_type FOREIGN KEY (component_type_id)
      REFERENCES pa_salary_component_type (component_type_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Danh muc TPL chuan cua he thong - nguoi dung chi ke thua, khong sua/xoa';

-- ============================================================
-- 4. pa_salary_composition
--    Danh sach TPL cua don vi - bang nghiep vu trung tam
-- ============================================================
CREATE TABLE pa_salary_composition (
  salary_composition_id CHAR(36)  NOT NULL,
  code             VARCHAR(255)   NOT NULL  COMMENT 'Ma TPL - duy nhat (BR-01), khong sua sau khi luu',
  name             VARCHAR(255)   NOT NULL  COMMENT 'Ten TPL - bat buoc <= 255 ky tu',
  organization_id  CHAR(36)       NULL,
  component_type_id CHAR(36)      NOT NULL,
  system_composition_id CHAR(36)  NULL      COMMENT 'NULL neu tu tao hoan toan',
  nature           TINYINT        NOT NULL  COMMENT '1=Thu nhap|2=Khau tru|3=Thong tin|4=Khac',
  tax_type         TINYINT        NULL      COMMENT '1=Chiu thue|2=Mien toan phan|3=Mien mot phan (BR-05: chi khi nature=1)',
  value_type       TINYINT        NOT NULL  DEFAULT 1 COMMENT '1=Tien te|2=So|3=Phan tram',
  value_mode       TINYINT        NOT NULL  DEFAULT 1 COMMENT '1=Tu dong cong tong NV|2=Theo cong thuc tu dat',
  value_formula    TEXT           NULL      COMMENT 'Cong thuc Gia tri (luu nguyen van chuoi)',
  value_scope      TINYINT        NULL,
  norm_formula     TEXT           NULL      COMMENT 'Cong thuc Dinh muc (muc tran)',
  allow_exceed_norm TINYINT(1)    NOT NULL  DEFAULT 0 COMMENT '1=cho phep vuot dinh muc (BR-09)',
  description      VARCHAR(1000)  NULL,
  show_on_payslip  TINYINT(1)     NOT NULL  DEFAULT 1,
  hide_when_zero   TINYINT(1)     NOT NULL  DEFAULT 0,
  source           TINYINT        NOT NULL  DEFAULT 1 COMMENT '1=Tu them|2=Ke thua he thong',
  status           TINYINT        NOT NULL  DEFAULT 1 COMMENT '1=Dang theo doi|0=Ngung theo doi (BR-07)',
  is_deleted       TINYINT(1)     NOT NULL  DEFAULT 0 COMMENT 'Xoa mem (BR-08)',
  created_date     DATETIME       NOT NULL  DEFAULT CURRENT_TIMESTAMP,
  created_by       VARCHAR(100)   NULL,
  modified_date    DATETIME       NULL      ON UPDATE CURRENT_TIMESTAMP,
  modified_by      VARCHAR(100)   NULL,
  PRIMARY KEY (salary_composition_id),
  UNIQUE KEY uq_comp_code (code),
  KEY idx_comp_org    (organization_id),
  KEY idx_comp_type   (component_type_id),
  KEY idx_comp_status (status),
  KEY idx_comp_deleted (is_deleted),
  KEY idx_comp_search  (code(50), name(50)),
  CONSTRAINT fk_comp_org  FOREIGN KEY (organization_id)
      REFERENCES pa_organization (organization_id),
  CONSTRAINT fk_comp_type FOREIGN KEY (component_type_id)
      REFERENCES pa_salary_component_type (component_type_id),
  CONSTRAINT fk_comp_sys  FOREIGN KEY (system_composition_id)
      REFERENCES pa_salary_composition_system (system_composition_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Danh sach TPL cua don vi - bang nghiep vu trung tam';

-- ============================================================
-- 5. pa_grid_config
--    Cau hinh cot bang theo nguoi dung (Bonus)
-- ============================================================
CREATE TABLE pa_grid_config (
  grid_config_id   CHAR(36)      NOT NULL,
  user_id          VARCHAR(100)  NOT NULL,
  grid_code        VARCHAR(100)  NOT NULL  COMMENT 'VD: SALARY_COMPOSITION_LIST',
  column_key       VARCHAR(100)  NOT NULL  COMMENT 'VD: code, name, status',
  caption          VARCHAR(255)  NULL,
  order_index      INT           NOT NULL  DEFAULT 0,
  width            INT           NULL      COMMENT 'Do rong cot (px)',
  is_pinned        TINYINT(1)    NOT NULL  DEFAULT 0,
  pin_position     TINYINT       NULL      COMMENT '1=ghin trai|2=ghin phai',
  is_visible       TINYINT(1)    NOT NULL  DEFAULT 1,
  created_date     DATETIME      NOT NULL  DEFAULT CURRENT_TIMESTAMP,
  modified_date    DATETIME      NULL      ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (grid_config_id),
  UNIQUE KEY uq_grid_user_col (user_id, grid_code, column_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Cau hinh cot bang (ghin/an-hien/do rong/thu tu) theo tung nguoi dung';

-- ============================================================
-- SEED DATA
-- ============================================================

-- ------------------------------------------------------------
-- 1. Loai thanh phan luong
-- ------------------------------------------------------------
INSERT INTO pa_salary_component_type
  (component_type_id, code, name, sort_order)
VALUES
  ('ctype001-0000-0000-0000-000000000001', 'LUONG',     'Luong',              1),
  ('ctype001-0000-0000-0000-000000000002', 'PHU_CAP',   'Phu cap',            2),
  ('ctype001-0000-0000-0000-000000000003', 'BAO_HIEM',  'Bao hiem',           3),
  ('ctype001-0000-0000-0000-000000000004', 'KHAU_TRU',  'Khau tru',           4),
  ('ctype001-0000-0000-0000-000000000005', 'CHAM_CONG', 'Cham cong',          5),
  ('ctype001-0000-0000-0000-000000000006', 'THUE_TNCN', 'Thue TNCN',          6),
  ('ctype001-0000-0000-0000-000000000007', 'KHAC',      'Khac',               7);

-- ------------------------------------------------------------
-- 2. Don vi cong tac
-- ------------------------------------------------------------
INSERT INTO pa_organization
  (organization_id, code, name, parent_id, path, sort_order, created_by)
VALUES
  ('org00001-0000-0000-0000-000000000001', 'CTY',  'Cong ty TNHH MISA',  NULL,                                   '/CTY/',       1, 'admin'),
  ('org00001-0000-0000-0000-000000000002', 'KD',   'Phong Kinh doanh',   'org00001-0000-0000-0000-000000000001', '/CTY/KD/',    1, 'admin'),
  ('org00001-0000-0000-0000-000000000003', 'KT',   'Phong Ke toan',      'org00001-0000-0000-0000-000000000001', '/CTY/KT/',    2, 'admin'),
  ('org00001-0000-0000-0000-000000000004', 'CNTT', 'Phong CNTT',         'org00001-0000-0000-0000-000000000001', '/CTY/CNTT/',  3, 'admin'),
  ('org00001-0000-0000-0000-000000000005', 'NS',   'Phong Nhan su',      'org00001-0000-0000-0000-000000000001', '/CTY/NS/',    4, 'admin');

-- ------------------------------------------------------------
-- 3. Danh muc TPL he thong (seed, chi doc)
-- nature: 1=Thu nhap | 2=Khau tru | 3=Thong tin | 4=Khac
-- tax_type: 1=Chiu thue | 2=Mien toan phan | 3=Mien mot phan
-- value_type: 1=Tien te | 2=So | 3=Phan tram
-- ------------------------------------------------------------
INSERT INTO pa_salary_composition_system
  (system_composition_id, code, name, component_type_id,
   nature, tax_type, value_type, default_value, norm_formula, description)
VALUES
  -- Thu nhap - Luong
  ('syscomp1-0000-0000-0000-000000000001',
   'LUONG_CO_BAN', 'Luong co ban',
   'ctype001-0000-0000-0000-000000000001',
   1, 1, 1, NULL, NULL,
   'Luong co ban theo hop dong lao dong, chiu thue TNCN'),

  ('syscomp1-0000-0000-0000-000000000002',
   'LUONG_DONG_BH', 'Luong dong bao hiem',
   'ctype001-0000-0000-0000-000000000001',
   1, 1, 1, NULL, '36000000',
   'Muc luong dong BHXH/BHYT/BHTN; toi da 20 lan luong co so nha nuoc'),

  -- Thu nhap - Phu cap
  ('syscomp1-0000-0000-0000-000000000003',
   'PHU_CAP_CHUC_VU', 'Phu cap chuc vu',
   'ctype001-0000-0000-0000-000000000002',
   1, 1, 1, NULL, NULL,
   'Phu cap theo chuc danh - chiu thue TNCN'),

  ('syscomp1-0000-0000-0000-000000000004',
   'PHU_CAP_DIEN_THOAI', 'Phu cap dien thoai',
   'ctype001-0000-0000-0000-000000000002',
   1, 2, 1, 500000.0000, NULL,
   'Co dinh 500.000 d/thang - khong chiu thue TNCN theo TT96/2015'),

  ('syscomp1-0000-0000-0000-000000000005',
   'PHU_CAP_AN_TRUA', 'Phu cap tien an trua',
   'ctype001-0000-0000-0000-000000000002',
   1, 2, 1, 730000.0000, '730000',
   'Toi da 730.000 d/thang khong chiu thue TNCN (TT96/2015)'),

  ('syscomp1-0000-0000-0000-000000000006',
   'PHU_CAP_XANG_XE', 'Phu cap xang xe',
   'ctype001-0000-0000-0000-000000000002',
   1, 2, 1, NULL, NULL,
   'Phu cap di lai theo quy che cong ty - khong chiu thue'),

  ('syscomp1-0000-0000-0000-000000000007',
   'PHU_CAP_CONG_TAC', 'Phu cap cong tac phi',
   'ctype001-0000-0000-0000-000000000002',
   1, 3, 1, NULL, NULL,
   'Cong tac phi thuc te - mien thue mot phan theo chung tu'),

  -- Khau tru - Bao hiem
  ('syscomp1-0000-0000-0000-000000000008',
   'BHXH_NLD', 'BHXH nguoi lao dong (8%)',
   'ctype001-0000-0000-0000-000000000003',
   2, NULL, 1, NULL, NULL,
   'NLD dong 8% muc luong dong BH; duoc giam tru tinh thue TNCN'),

  ('syscomp1-0000-0000-0000-000000000009',
   'BHYT_NLD', 'BHYT nguoi lao dong (1.5%)',
   'ctype001-0000-0000-0000-000000000003',
   2, NULL, 1, NULL, NULL,
   'NLD dong 1.5% muc luong dong BH'),

  ('syscomp1-0000-0000-0000-000000000010',
   'BHTN_NLD', 'BHTN nguoi lao dong (1%)',
   'ctype001-0000-0000-0000-000000000003',
   2, NULL, 1, NULL, NULL,
   'NLD dong 1% muc luong dong BH'),

  -- Khau tru - Thue TNCN
  ('syscomp1-0000-0000-0000-000000000011',
   'THUE_TNCN', 'Thue thu nhap ca nhan',
   'ctype001-0000-0000-0000-000000000006',
   2, NULL, 1, NULL, NULL,
   'Tinh theo bieu luy tien tung phan; giam tru ban than 11tr, NPT 4.4tr/nguoi'),

  -- Thong tin / Can cu tinh luong (nature=3)
  ('syscomp1-0000-0000-0000-000000000012',
   'SO_CONG_CHUAN', 'So cong chuan',
   'ctype001-0000-0000-0000-000000000005',
   3, NULL, 2, NULL, NULL,
   'Tong so ngay cong chuan trong ky luong'),

  ('syscomp1-0000-0000-0000-000000000013',
   'SO_CONG_THUC_TE', 'So cong thuc te',
   'ctype001-0000-0000-0000-000000000005',
   3, NULL, 2, NULL, NULL,
   'So ngay cong thuc te nhan vien di lam'),

  ('syscomp1-0000-0000-0000-000000000014',
   'SO_NGUOI_PHU_THUOC', 'So nguoi phu thuoc',
   'ctype001-0000-0000-0000-000000000007',
   3, NULL, 2, NULL, NULL,
   'So nguoi phu thuoc dang ky giam tru gia canh'),

  -- Tong hop bat buoc (nature=4)
  ('syscomp1-0000-0000-0000-000000000015',
   'TONG_THU_NHAP', 'Tong thu nhap',
   'ctype001-0000-0000-0000-000000000001',
   4, NULL, 1, NULL, NULL,
   'Tong tat ca khoan thu nhap truoc khau tru - Bat buoc tren bang luong'),

  ('syscomp1-0000-0000-0000-000000000016',
   'TONG_KHAU_TRU', 'Tong khau tru',
   'ctype001-0000-0000-0000-000000000004',
   4, NULL, 1, NULL, NULL,
   'Tong tat ca khoan khau tru - Bat buoc tren bang luong'),

  ('syscomp1-0000-0000-0000-000000000017',
   'LUONG_KY_NAY', 'Luong ky nay',
   'ctype001-0000-0000-0000-000000000001',
   4, NULL, 1, NULL, NULL,
   'Thu nhap ky nay truoc tam ung - Bat buoc tren bang luong'),

  ('syscomp1-0000-0000-0000-000000000018',
   'TAM_UNG', 'Tam ung',
   'ctype001-0000-0000-0000-000000000004',
   4, NULL, 1, NULL, NULL,
   'So tien da tam ung trong ky - Bat buoc tren bang luong'),

  ('syscomp1-0000-0000-0000-000000000019',
   'THUC_LINH', 'Thuc linh',
   'ctype001-0000-0000-0000-000000000001',
   4, NULL, 1, NULL, NULL,
   'So tien nhan vien thuc nhan = Luong ky nay - Tam ung - Bat buoc');

-- ------------------------------------------------------------
-- 4. TPL cua don vi
-- source: 1=Tu them | 2=Ke thua he thong
-- status: 1=Dang theo doi | 0=Ngung theo doi
-- is_deleted: 0=binh thuong | 1=da xoa mem (BR-08)
-- ------------------------------------------------------------

-- ---- Ke thua tu danh muc he thong (source = 2) -------------
INSERT INTO pa_salary_composition
  (salary_composition_id, code, name,
   organization_id, component_type_id, system_composition_id,
   nature, tax_type, value_type, value_mode, value_formula,
   norm_formula, allow_exceed_norm, description,
   show_on_payslip, hide_when_zero, source, status, is_deleted, created_by)
VALUES
  ('scomp001-0000-0000-0000-000000000001',
   'LUONG_CO_BAN', 'Luong co ban',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000001',
   'syscomp1-0000-0000-0000-000000000001',
   1, 1, 1, 1, NULL, NULL, 0,
   'Luong co ban theo hop dong lao dong',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000002',
   'LUONG_DONG_BH', 'Luong dong bao hiem',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000001',
   'syscomp1-0000-0000-0000-000000000002',
   1, 1, 1, 1, NULL, '36000000', 0,
   'Muc luong dong BH; dinh muc = 20 x luong co so nha nuoc',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000003',
   'PHU_CAP_CHUC_VU', 'Phu cap chuc vu',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000002',
   'syscomp1-0000-0000-0000-000000000003',
   1, 1, 1, 1, NULL, NULL, 0,
   'Phu cap theo chuc danh',
   1, 1, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000004',
   'PHU_CAP_DIEN_THOAI', 'Phu cap dien thoai',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000002',
   'syscomp1-0000-0000-0000-000000000004',
   1, 2, 1, 2, '500000', '1000000', 0,
   'Co dinh 500.000 d/thang; toi da 1.000.000 d - khong chiu thue TNCN',
   1, 1, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000005',
   'PHU_CAP_AN_TRUA', 'Phu cap tien an trua',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000002',
   'syscomp1-0000-0000-0000-000000000005',
   1, 2, 1, 2, '730000', '730000', 0,
   'Co dinh 730.000 d/thang - khong chiu thue TNCN',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000006',
   'PHU_CAP_XANG_XE', 'Phu cap xang xe',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000002',
   'syscomp1-0000-0000-0000-000000000006',
   1, 2, 1, 1, NULL, NULL, 0,
   'Phu cap di lai theo quy che cong ty',
   1, 1, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000007',
   'BHXH_NLD', 'BHXH nguoi lao dong (8%)',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000003',
   'syscomp1-0000-0000-0000-000000000008',
   2, NULL, 1, 2, 'LUONG_DONG_BH * 0.08', NULL, 0,
   'NLD dong 8% luong dong BH - duoc giam tru thue TNCN',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000008',
   'BHYT_NLD', 'BHYT nguoi lao dong (1.5%)',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000003',
   'syscomp1-0000-0000-0000-000000000009',
   2, NULL, 1, 2, 'LUONG_DONG_BH * 0.015', NULL, 0,
   'NLD dong 1.5% luong dong BH',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000009',
   'BHTN_NLD', 'BHTN nguoi lao dong (1%)',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000003',
   'syscomp1-0000-0000-0000-000000000010',
   2, NULL, 1, 2, 'LUONG_DONG_BH * 0.01', NULL, 0,
   'NLD dong 1% luong dong BH',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000010',
   'THUE_TNCN', 'Thue thu nhap ca nhan',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000006',
   'syscomp1-0000-0000-0000-000000000011',
   2, NULL, 1, 2,
   'IF((TONG_THU_NHAP-BHXH_NLD-BHYT_NLD-BHTN_NLD-11000000-SO_NGUOI_PHU_THUOC*4400000)>0, ROUND((TONG_THU_NHAP-BHXH_NLD-BHYT_NLD-BHTN_NLD-11000000-SO_NGUOI_PHU_THUOC*4400000)*0.1,0), 0)',
   NULL, 0,
   'Vi du don gian bac 1 (10%) - thuc te tinh du 7 bac luy tien',
   1, 1, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000011',
   'SO_CONG_THUC_TE', 'So cong thuc te',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000005',
   'syscomp1-0000-0000-0000-000000000013',
   3, NULL, 2, 1, NULL, NULL, 0,
   'So ngay cong thuc te di lam trong ky',
   0, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000012',
   'SO_NGUOI_PHU_THUOC', 'So nguoi phu thuoc',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000007',
   'syscomp1-0000-0000-0000-000000000014',
   3, NULL, 2, 1, NULL, NULL, 0,
   'So nguoi phu thuoc dang ky giam tru gia canh',
   0, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000013',
   'TONG_THU_NHAP', 'Tong thu nhap',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000001',
   'syscomp1-0000-0000-0000-000000000015',
   4, NULL, 1, 2,
   'SUM(LUONG_CO_BAN, PHU_CAP_CHUC_VU, PHU_CAP_DIEN_THOAI, PHU_CAP_AN_TRUA, LUONG_KPI_KD, THUONG_DOANH_SO)',
   NULL, 0,
   'Tong tat ca khoan thu nhap truoc khau tru',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000014',
   'TONG_KHAU_TRU', 'Tong khau tru',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000004',
   'syscomp1-0000-0000-0000-000000000016',
   4, NULL, 1, 2,
   'SUM(BHXH_NLD, BHYT_NLD, BHTN_NLD, THUE_TNCN)',
   NULL, 0,
   'Tong tat ca khoan khau tru',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000015',
   'LUONG_KY_NAY', 'Luong ky nay',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000001',
   'syscomp1-0000-0000-0000-000000000017',
   4, NULL, 1, 2, 'TONG_THU_NHAP - TONG_KHAU_TRU',
   NULL, 0,
   'Thu nhap ky nay truoc tam ung',
   1, 0, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000016',
   'TAM_UNG', 'Tam ung',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000004',
   'syscomp1-0000-0000-0000-000000000018',
   4, NULL, 1, 1, NULL, NULL, 0,
   'So tien da tam ung trong ky',
   1, 1, 2, 1, 0, 'hr_admin'),

  ('scomp001-0000-0000-0000-000000000017',
   'THUC_LINH', 'Thuc linh',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000001',
   'syscomp1-0000-0000-0000-000000000019',
   4, NULL, 1, 2, 'LUONG_KY_NAY - TAM_UNG',
   NULL, 0,
   'So tien nhan vien thuc nhan',
   1, 0, 2, 1, 0, 'hr_admin');

-- ---- Tu them - rieng don vi (source = 1) --------------------
INSERT INTO pa_salary_composition
  (salary_composition_id, code, name,
   organization_id, component_type_id, system_composition_id,
   nature, tax_type, value_type, value_mode, value_formula,
   norm_formula, allow_exceed_norm, description,
   show_on_payslip, hide_when_zero, source, status, is_deleted, created_by)
VALUES
  -- Luong KPI - Phong Kinh doanh
  ('scomp002-0000-0000-0000-000000000001',
   'LUONG_KPI_KD', 'Luong KPI phong Kinh doanh',
   40290,
   'ctype001-0000-0000-0000-000000000001',
   NULL,
   1, 1, 1, 2,
   'ROUND(LUONG_CO_BAN * TY_LE_KPI, 0)',
   NULL, 1,
   'Luong bien doi theo % hoan thanh KPI; vuot dinh muc khi KPI > 100%',
   1, 1, 1, 1, 0, 'hr_admin'),

  -- Thuong doanh so - Phong Kinh doanh
  ('scomp002-0000-0000-0000-000000000002',
   'THUONG_DOANH_SO', 'Thuong doanh so',
   40290,
   'ctype001-0000-0000-0000-000000000001',
   NULL,
   1, 1, 1, 2,
   'IF(AND(DOANH_SO>1000000000,SO_KHACH_HANG_MOI>50), DOANH_SO*0.005, DOANH_SO*0.002)',
   NULL, 0,
   'Thuong 0.5% DS khi DS>1 ty VA KH moi>50; nguoc lai 0.2%',
   1, 1, 1, 1, 0, 'hr_admin'),

  -- Ty le KPI (thong tin can cu)
  ('scomp002-0000-0000-0000-000000000003',
   'TY_LE_KPI', 'Ty le hoan thanh KPI',
   40290,
   'ctype001-0000-0000-0000-000000000007',
   NULL,
   3, NULL, 3, 1, NULL, NULL, 0,
   'Ty le % dat KPI trong ky; VD: 0.95 = 95%',
   0, 0, 1, 1, 0, 'hr_admin'),

  -- Luong lam them gio - Phong CNTT
  ('scomp002-0000-0000-0000-000000000004',
   'LUONG_LAM_THEM', 'Luong lam them gio',
   'org00001-0000-0000-0000-000000000004',
   'ctype001-0000-0000-0000-000000000001',
   NULL,
   1, 1, 1, 2,
   'ROUND((LUONG_CO_BAN / SO_CONG_CHUAN / 8) * SO_GIO_LAM_THEM * 1.5, 0)',
   NULL, 0,
   'Luong OT ngay thuong x 1.5 theo Bo luat Lao dong 2019',
   1, 1, 1, 1, 0, 'hr_admin'),

  -- Phat di muon - Toan cong ty
  ('scomp002-0000-0000-0000-000000000005',
   'PHAT_DI_MUON', 'Phat di muon',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000004',
   NULL,
   2, NULL, 1, 2,
   'SO_LAN_DI_MUON * 50000',
   NULL, 0,
   'Moi lan di muon tru 50.000 d; khong duoc giam tru thue',
   1, 1, 1, 1, 0, 'hr_admin'),

  -- Vi du TPL ngung theo doi (status = 0) - BR-07
  ('scomp002-0000-0000-0000-000000000006',
   'PC_NANG_SUAT_CU', 'Phu cap nang suat cu',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000002',
   NULL,
   1, 1, 1, 2, '1000000', NULL, 0,
   'Da thay bang chinh sach KPI moi tu nam 2025',
   0, 0, 1, 0, 0, 'hr_admin'),

  -- Vi du TPL da xoa mem (is_deleted = 1) - BR-08
  ('scomp002-0000-0000-0000-000000000007',
   'PC_TEST_XOA_MEM', 'Phu cap thu nghiem da xoa',
   'org00001-0000-0000-0000-000000000001',
   'ctype001-0000-0000-0000-000000000002',
   NULL,
   1, 2, 1, 2, '200000', NULL, 0,
   'Ban ghi nay da bi xoa mem - khong hien tren danh sach',
   1, 1, 1, 1, 1, 'hr_admin');

-- ------------------------------------------------------------
-- 5. pa_grid_config - cau hinh cot bang (Bonus)
-- ------------------------------------------------------------
INSERT INTO pa_grid_config
  (grid_config_id, user_id, grid_code, column_key, caption,
   order_index, width, is_pinned, pin_position, is_visible)
VALUES
  ('gridcfg1-0000-0000-0000-000000000001', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'code',             'Ma thanh phan',   1, 150, 1, 1, 1),
  ('gridcfg1-0000-0000-0000-000000000002', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'name',             'Ten thanh phan',  2, 260, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000003', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'organization',     'Don vi ap dung',  3, 180, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000004', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'component_type',   'Loai thanh phan', 4, 140, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000005', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'nature',           'Tinh chat',       5, 120, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000006', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'value_type',       'Kieu gia tri',    6, 110, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000007', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'value_formula',    'Gia tri',         7, 240, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000008', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'source',           'Nguon tao',       8, 110, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000009', 'hr_user_01', 'SALARY_COMPOSITION_LIST', 'status',           'Trang thai',      9, 130, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000010', 'hr_user_02', 'SALARY_COMPOSITION_LIST', 'code',             'Ma thanh phan',   1, 150, 1, 1, 1),
  ('gridcfg1-0000-0000-0000-000000000011', 'hr_user_02', 'SALARY_COMPOSITION_LIST', 'name',             'Ten thanh phan',  2, 300, 0, NULL, 1),
  ('gridcfg1-0000-0000-0000-000000000012', 'hr_user_02', 'SALARY_COMPOSITION_LIST', 'value_formula',    'Gia tri',         3, 240, 0, NULL, 0);

-- ============================================================
-- KHOI PHUC CAU HINH
-- ============================================================
SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;
SET SQL_MODE = @OLD_SQL_MODE;

-- ============================================================
-- KIEM TRA NHANH SAU KHI CHAY
-- ============================================================
SELECT 'pa_organization'              AS bang, COUNT(*) AS so_ban_ghi FROM pa_organization
UNION ALL
SELECT 'pa_salary_component_type',    COUNT(*) FROM pa_salary_component_type
UNION ALL
SELECT 'pa_salary_composition_system',COUNT(*) FROM pa_salary_composition_system
UNION ALL
SELECT 'pa_salary_composition',       COUNT(*) FROM pa_salary_composition
UNION ALL
SELECT 'pa_grid_config',              COUNT(*) FROM pa_grid_config;

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   