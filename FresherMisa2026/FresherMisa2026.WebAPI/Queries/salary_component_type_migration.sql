-- ============================================================
-- AMIS TIEN LUONG - SALARY COMPONENT TYPE
-- Database: amis_tien_luong
-- Chạy: mysql -u root -p amis_tien_luong < salary_component_type_migration.sql
--
-- Tên proc theo convention BaseRepository: Proc_{tableName}_*
-- tableName = "pa_salary_component_type" (theo [ConfigTable])
-- hasDeletedColumn = false → xóa cứng
-- ============================================================

SET NAMES utf8mb4;

USE amis_tien_luong;

-- ============================================================
-- TABLE: pa_salary_component_type
-- ============================================================
DROP TABLE IF EXISTS `pa_salary_component_type`;
CREATE TABLE `pa_salary_component_type` (
  `ComponentTypeID`   CHAR(36)        NOT NULL            COMMENT 'Khóa chính',
  `Code`              VARCHAR(255)    NOT NULL            COMMENT 'Mã loại thành phần — unique',
  `Name`              VARCHAR(255)    NOT NULL            COMMENT 'Tên loại thành phần',
  `SortOrder`         INT             NULL                COMMENT 'Thứ tự hiển thị',
  `IsActive`          TINYINT(1)      NOT NULL DEFAULT 1  COMMENT '1=Đang hoạt động|0=Ngừng hoạt động',
  `CreatedBy`         VARCHAR(100)    NULL,
  `CreateDate`        DATETIME        NULL,
  `ModifiedBy`        VARCHAR(100)    NULL,
  `ModifiedDate`      DATETIME        NULL,
  PRIMARY KEY (`ComponentTypeID`),
  UNIQUE KEY `UQ_Code` (`Code`),
  KEY `IDX_IsActive`  (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Loại thành phần lương (Lương cơ bản, Phụ cấp, Bảo hiểm...)';

-- ============================================================
-- PROC: Proc_Insertpa_salary_component_type
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Insertpa_salary_component_type`;
delimiter ;;
CREATE PROCEDURE `Proc_Insertpa_salary_component_type`(
  IN v_ComponentTypeID  CHAR(36),
  IN v_Code             VARCHAR(255),
  IN v_Name             VARCHAR(255),
  IN v_SortOrder        INT,
  IN v_IsActive         TINYINT(1),
  IN v_CreatedBy        VARCHAR(100),
  IN v_CreateDate       DATETIME
)
BEGIN
  INSERT INTO pa_salary_component_type (
    ComponentTypeID, Code, Name, SortOrder, IsActive, CreatedBy, CreateDate
  ) VALUES (
    v_ComponentTypeID, v_Code, v_Name, v_SortOrder, IFNULL(v_IsActive, 1), v_CreatedBy, v_CreateDate
  );
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Updatepa_salary_component_type
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Updatepa_salary_component_type`;
delimiter ;;
CREATE PROCEDURE `Proc_Updatepa_salary_component_type`(
  IN v_ComponentTypeID  CHAR(36),
  IN v_Code             VARCHAR(255),
  IN v_Name             VARCHAR(255),
  IN v_SortOrder        INT,
  IN v_IsActive         TINYINT(1),
  IN v_ModifiedBy       VARCHAR(100),
  IN v_ModifiedDate     DATETIME
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_salary_component_type WHERE ComponentTypeID = v_ComponentTypeID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thành phần lương không tồn tại';
  END IF;

  UPDATE pa_salary_component_type
  SET Code         = v_Code,
      Name         = v_Name,
      SortOrder    = v_SortOrder,
      IsActive     = v_IsActive,
      ModifiedBy   = v_ModifiedBy,
      ModifiedDate = v_ModifiedDate
  WHERE ComponentTypeID = v_ComponentTypeID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Deletepa_salary_component_typeById (xóa cứng)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Deletepa_salary_component_typeById`;
delimiter ;;
CREATE PROCEDURE `Proc_Deletepa_salary_component_typeById`(
  IN v_ComponentTypeID CHAR(36)
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_salary_component_type WHERE ComponentTypeID = v_ComponentTypeID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thành phần lương không tồn tại';
  END IF;

  DELETE FROM pa_salary_component_type WHERE ComponentTypeID = v_ComponentTypeID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_pa_salary_component_type_FilterPaging (generic paging)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_pa_salary_component_type_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_pa_salary_component_type_FilterPaging`(
  IN v_pageIndex    INT,
  IN v_pageSize     INT,
  IN v_search       VARCHAR(255),
  IN v_sort         VARCHAR(200),
  IN v_searchFields JSON
)
BEGIN
  DECLARE v_offset          INT;
  DECLARE v_where           TEXT DEFAULT ' WHERE 1=1 ';
  DECLARE v_orderBy         TEXT DEFAULT '';
  DECLARE v_searchCondition TEXT;

  IF v_pageIndex < 1 THEN SET v_pageIndex = 1; END IF;
  IF v_pageSize  < 1 THEN SET v_pageSize  = 20; END IF;
  SET v_offset = (v_pageIndex - 1) * v_pageSize;

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
    SET v_orderBy = ' ORDER BY SortOrder ASC, CreateDate DESC';
  END IF;

  SET @v_sql = CONCAT(
    'SELECT * FROM pa_salary_component_type', v_where, v_orderBy,
    ' LIMIT ', v_offset, ',', v_pageSize
  );
  SET @v_sqlCount = CONCAT('SELECT COUNT(*) AS Total FROM pa_salary_component_type', v_where);

  PREPARE stmt FROM @v_sql;      EXECUTE stmt;      DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @v_sqlCount; EXECUTE stmt;      DEALLOCATE PREPARE stmt;
END
;;
delimiter ;
