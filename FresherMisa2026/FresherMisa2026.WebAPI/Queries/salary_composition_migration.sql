-- ============================================================
-- AMIS TIEN LUONG - SALARY COMPOSITION (entity chính)
-- Database: amis_tien_luong
-- Chạy: mysql -u root -p amis_tien_luong < salary_composition_migration.sql
--
-- Tên proc theo convention BaseRepository: Proc_{tableName}_*
-- tableName = "pa_salary_composition" (theo [ConfigTable])
-- ============================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE DATABASE IF NOT EXISTS amis_tien_luong
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE amis_tien_luong;

-- ============================================================
-- TABLE: pa_salary_composition
-- ============================================================
DROP TABLE IF EXISTS `pa_salary_composition`;
CREATE TABLE `pa_salary_composition` (
  `SalaryCompositionID`   CHAR(36)        NOT NULL            COMMENT 'Khóa chính',
  `Code`                  VARCHAR(255)    NOT NULL            COMMENT 'Mã TPL — không sửa sau khi lưu (BR-01)',
  `Name`                  VARCHAR(255)    NOT NULL            COMMENT 'Tên TPL',
  `OrganizationID`        CHAR(36)        NULL                COMMENT 'Đơn vị (NULL = áp dụng toàn công ty)',
  `ComponentTypeID`       CHAR(36)        NOT NULL            COMMENT 'Loại thành phần lương',
  `SystemCompositionID`   CHAR(36)        NULL                COMMENT 'NULL nếu tự tạo hoàn toàn',
  `Nature`                TINYINT         NOT NULL            COMMENT '1=Thu nhập|2=Khấu trừ|3=Thông tin|4=Khác',
  `TaxType`               TINYINT         NULL                COMMENT '1=Chịu thuế|2=Miễn toàn phần|3=Miễn một phần (BR-05: chỉ khi Nature=1)',
  `ValueType`             TINYINT         NOT NULL DEFAULT 1  COMMENT '1=Tiền tệ|2=Số|3=Phần trăm',
  `ValueMode`             TINYINT         NOT NULL DEFAULT 1  COMMENT '1=Tự động cộng tổng|2=Theo công thức',
  `ValueFormula`          TEXT            NULL                COMMENT 'Công thức tính giá trị (lưu nguyên văn chuỗi)',
  `ValueScope`            INT             NULL,
  `NormFormula`           TEXT            NULL                COMMENT 'Công thức định mức — mức trần',
  `AllowExceedNorm`       TINYINT(1)      NOT NULL DEFAULT 0  COMMENT 'BR-09: cho phép vượt định mức',
  `Description`           VARCHAR(1000)   NULL,
  `ShowOnPayslip`         TINYINT(1)      NOT NULL DEFAULT 1,
  `HideWhenZero`          TINYINT(1)      NOT NULL DEFAULT 0,
  `Source`                TINYINT         NOT NULL DEFAULT 1  COMMENT '1=Tự thêm|2=Kế thừa hệ thống',
  `Status`                TINYINT         NOT NULL DEFAULT 1  COMMENT '1=Đang theo dõi|0=Ngừng theo dõi (BR-07)',
  `IsDeleted`             TINYINT(1)      NOT NULL DEFAULT 0,
  `CreatedBy`             VARCHAR(100)    NULL,
  `CreateDate`            DATETIME        NULL,
  `ModifiedBy`            VARCHAR(100)    NULL,
  `ModifiedDate`          DATETIME        NULL,
  PRIMARY KEY (`SalaryCompositionID`),
  UNIQUE KEY `UQ_Code` (`Code`),
  KEY `IDX_Nature`        (`Nature`),
  KEY `IDX_Status`        (`Status`),
  KEY `IDX_IsDeleted`     (`IsDeleted`),
  KEY `IDX_ComponentType` (`ComponentTypeID`),
  KEY `IDX_Organization`  (`OrganizationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Thành phần lương của đơn vị — bảng nghiệp vụ trung tâm';

-- ============================================================
-- PROC: Proc_Insertpa_salary_composition
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Insertpa_salary_composition`;
delimiter ;;
CREATE PROCEDURE `Proc_Insertpa_salary_composition`(
  IN v_SalaryCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_OrganizationID       CHAR(36),
  IN v_ComponentTypeID      CHAR(36),
  IN v_SystemCompositionID  CHAR(36),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_ValueType            TINYINT,
  IN v_ValueMode            TINYINT,
  IN v_ValueFormula         TEXT,
  IN v_ValueScope           INT,
  IN v_NormFormula          TEXT,
  IN v_AllowExceedNorm      TINYINT(1),
  IN v_Description          VARCHAR(1000),
  IN v_ShowOnPayslip        TINYINT(1),
  IN v_HideWhenZero         TINYINT(1),
  IN v_Source               TINYINT,
  IN v_Status               TINYINT,
  IN v_CreatedBy            VARCHAR(100),
  IN v_CreateDate           DATETIME
)
BEGIN
  -- BR-05: TaxType chỉ có ý nghĩa khi Nature = 1 (Thu nhập)
  IF v_TaxType IS NOT NULL AND v_Nature <> 1 THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập';
  END IF;

  INSERT INTO pa_salary_composition (
    SalaryCompositionID, Code, Name, OrganizationID, ComponentTypeID,
    SystemCompositionID, Nature, TaxType, ValueType, ValueMode,
    ValueFormula, ValueScope, NormFormula, AllowExceedNorm, Description,
    ShowOnPayslip, HideWhenZero, Source, Status, IsDeleted,
    CreatedBy, CreateDate
  ) VALUES (
    v_SalaryCompositionID, v_Code, v_Name, v_OrganizationID, v_ComponentTypeID,
    v_SystemCompositionID, v_Nature, v_TaxType, v_ValueType, v_ValueMode,
    v_ValueFormula, v_ValueScope, v_NormFormula, v_AllowExceedNorm, v_Description,
    v_ShowOnPayslip, v_HideWhenZero, v_Source, v_Status, 0,
    v_CreatedBy, v_CreateDate
  );
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Updatepa_salary_composition
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Updatepa_salary_composition`;
delimiter ;;
CREATE PROCEDURE `Proc_Updatepa_salary_composition`(
  IN v_SalaryCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_OrganizationID       CHAR(36),
  IN v_ComponentTypeID      CHAR(36),
  IN v_SystemCompositionID  CHAR(36),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_ValueType            TINYINT,
  IN v_ValueMode            TINYINT,
  IN v_ValueFormula         TEXT,
  IN v_ValueScope           INT,
  IN v_NormFormula          TEXT,
  IN v_AllowExceedNorm      TINYINT(1),
  IN v_Description          VARCHAR(1000),
  IN v_ShowOnPayslip        TINYINT(1),
  IN v_HideWhenZero         TINYINT(1),
  IN v_Source               TINYINT,
  IN v_Status               TINYINT,
  IN v_ModifiedBy           VARCHAR(100),
  IN v_ModifiedDate         DATETIME
)
BEGIN
  DECLARE v_existingCode VARCHAR(255);

  IF NOT EXISTS (SELECT 1 FROM pa_salary_composition WHERE SalaryCompositionID = v_SalaryCompositionID AND IsDeleted = 0) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Thành phần lương không tồn tại';
  END IF;

  -- BR-01: Code không được sửa sau khi lưu
  SELECT Code INTO v_existingCode
  FROM pa_salary_composition
  WHERE SalaryCompositionID = v_SalaryCompositionID;

  IF v_existingCode <> v_Code THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Mã thành phần lương không được thay đổi sau khi lưu';
  END IF;

  -- BR-05: TaxType chỉ có ý nghĩa khi Nature = 1
  IF v_TaxType IS NOT NULL AND v_Nature <> 1 THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập';
  END IF;

  UPDATE pa_salary_composition
  SET Name                = v_Name,
      OrganizationID      = v_OrganizationID,
      ComponentTypeID     = v_ComponentTypeID,
      SystemCompositionID = v_SystemCompositionID,
      Nature              = v_Nature,
      TaxType             = v_TaxType,
      ValueType           = v_ValueType,
      ValueMode           = v_ValueMode,
      ValueFormula        = v_ValueFormula,
      ValueScope          = v_ValueScope,
      NormFormula         = v_NormFormula,
      AllowExceedNorm     = v_AllowExceedNorm,
      Description         = v_Description,
      ShowOnPayslip       = v_ShowOnPayslip,
      HideWhenZero        = v_HideWhenZero,
      Source              = v_Source,
      Status              = v_Status,
      ModifiedBy          = v_ModifiedBy,
      ModifiedDate        = v_ModifiedDate
  WHERE SalaryCompositionID = v_SalaryCompositionID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Deletepa_salary_compositionById (xóa mềm)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Deletepa_salary_compositionById`;
delimiter ;;
CREATE PROCEDURE `Proc_Deletepa_salary_compositionById`(
  IN v_SalaryCompositionID CHAR(36)
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_salary_composition WHERE SalaryCompositionID = v_SalaryCompositionID AND IsDeleted = 0) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Thành phần lương không tồn tại';
  END IF;

  -- BR-08: TPL kế thừa từ hệ thống không được xóa
  IF EXISTS (SELECT 1 FROM pa_salary_composition WHERE SalaryCompositionID = v_SalaryCompositionID AND Source = 2) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Không thể xóa thành phần lương mặc định của hệ thống';
  END IF;

  UPDATE pa_salary_composition
  SET IsDeleted = 1, ModifiedDate = NOW()
  WHERE SalaryCompositionID = v_SalaryCompositionID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_pa_salary_composition_FilterPaging (generic paging)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_pa_salary_composition_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_pa_salary_composition_FilterPaging`(
  IN v_pageIndex    INT,
  IN v_pageSize     INT,
  IN v_search       VARCHAR(255),
  IN v_sort         VARCHAR(200),
  IN v_searchFields JSON
)
BEGIN
  DECLARE v_offset          INT;
  DECLARE v_where           TEXT DEFAULT ' WHERE IsDeleted = 0 ';
  DECLARE v_orderBy         TEXT DEFAULT '';
  DECLARE v_searchCondition TEXT;

  IF v_pageIndex < 1 THEN SET v_pageIndex = 1; END IF;
  IF v_pageSize  < 1 THEN SET v_pageSize  = 20; END IF;
  SET v_offset = (v_pageIndex - 1) * v_pageSize;

  -- Search nhiều trường
  IF v_search IS NOT NULL AND v_search <> '' AND v_searchFields IS NOT NULL THEN
    SELECT GROUP_CONCAT(
      CONCAT('`', JSON_UNQUOTE(JSON_EXTRACT(v_searchFields, CONCAT('$[', n, ']'))), '` LIKE "%', v_search, '%"')
      SEPARATOR ' OR '
    ) INTO v_searchCondition
    FROM (
      SELECT 0 n UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4
      UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9
    ) t
    WHERE n < JSON_LENGTH(v_searchFields);

    IF v_searchCondition IS NOT NULL THEN
      SET v_where = CONCAT(v_where, ' AND (', v_searchCondition, ')');
    END IF;
  END IF;

  -- Sort
  IF v_sort IS NOT NULL AND v_sort <> '' THEN
    SELECT GROUP_CONCAT(
      CONCAT('`', SUBSTRING(item, 2), '` ', IF(LEFT(item, 1) = '-', 'DESC', 'ASC'))
      SEPARATOR ', '
    ) INTO v_orderBy
    FROM (
      SELECT TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(v_sort, ',', n), ',', -1)) item
      FROM (SELECT 1 n UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5) x
      WHERE n <= 1 + LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', ''))
    ) y;

    IF v_orderBy IS NOT NULL THEN
      SET v_orderBy = CONCAT(' ORDER BY ', v_orderBy);
    END IF;
  END IF;

  IF v_orderBy IS NULL OR v_orderBy = '' THEN
    SET v_orderBy = ' ORDER BY CreateDate DESC';
  END IF;

  SET @v_sql = CONCAT(
    'SELECT * FROM pa_salary_composition', v_where, v_orderBy,
    ' LIMIT ', v_offset, ',', v_pageSize
  );
  SET @v_sqlCount = CONCAT('SELECT COUNT(*) AS Total FROM pa_salary_composition', v_where);

  PREPARE stmt FROM @v_sql;      EXECUTE stmt;      DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @v_sqlCount; EXECUTE stmt;      DEALLOCATE PREPARE stmt;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_pa_salary_composition_Filter (custom filter)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_pa_salary_composition_Filter`;
delimiter ;;
CREATE PROCEDURE `Proc_pa_salary_composition_Filter`(
  IN v_Search           VARCHAR(255),
  IN v_OrganizationIDs  TEXT,
  IN v_ComponentTypeID  CHAR(36),
  IN v_Nature           TINYINT,
  IN v_Status           TINYINT,
  IN v_Source           TINYINT,
  IN v_PageIndex        INT,
  IN v_PageSize         INT
)
BEGIN
  DECLARE v_offset INT;

  IF v_PageIndex < 1 THEN SET v_PageIndex = 1; END IF;
  IF v_PageSize  < 1 THEN SET v_PageSize  = 20; END IF;
  SET v_offset = (v_PageIndex - 1) * v_PageSize;

  SET @sql      = 'SELECT * FROM pa_salary_composition WHERE IsDeleted = 0';
  SET @sqlCount = 'SELECT COUNT(*) AS Total FROM pa_salary_composition WHERE IsDeleted = 0';
  SET @cond     = '';

  IF v_Search IS NOT NULL AND v_Search <> '' THEN
    SET @cond = CONCAT(@cond, ' AND (Code LIKE "%', v_Search, '%" OR Name LIKE "%', v_Search, '%")');
  END IF;
  IF v_OrganizationIDs IS NOT NULL AND JSON_LENGTH(v_OrganizationIDs) > 0 THEN
    SET @cond = CONCAT(@cond, ' AND JSON_CONTAINS(''', v_OrganizationIDs, ''', JSON_QUOTE(OrganizationID))');
  END IF;
  IF v_ComponentTypeID IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND ComponentTypeID = "', v_ComponentTypeID, '"');
  END IF;
  IF v_Nature IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND Nature = ', v_Nature);
  END IF;
  IF v_Status IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND Status = ', v_Status);
  END IF;
  IF v_Source IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND Source = ', v_Source);
  END IF;

  SET @sql      = CONCAT(@sql,      @cond, ' ORDER BY CreateDate DESC LIMIT ', v_offset, ',', v_PageSize);
  SET @sqlCount = CONCAT(@sqlCount, @cond);

  PREPARE stmt FROM @sql;      EXECUTE stmt;      DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @sqlCount; EXECUTE stmt;      DEALLOCATE PREPARE stmt;
END
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
