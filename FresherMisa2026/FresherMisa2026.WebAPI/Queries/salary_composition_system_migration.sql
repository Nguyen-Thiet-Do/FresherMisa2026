-- ============================================================
-- AMIS TIEN LUONG - SALARY COMPOSITION SYSTEM
-- Database: amis_tien_luong
-- Chạy: mysql -u root -p amis_tien_luong < salary_composition_system_migration.sql
--
-- Tên proc theo convention BaseRepository: Proc_{tableName}_*
-- tableName = "pa_salary_composition_system" (theo [ConfigTable])
-- hasDeletedColumn = false → xóa cứng
-- Dependency: pa_salary_component_type phải tồn tại trước
-- ============================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

USE amis_tien_luong;

-- ============================================================
-- TABLE: pa_salary_composition_system
-- ============================================================
DROP TABLE IF EXISTS `pa_salary_composition_system`;
CREATE TABLE `pa_salary_composition_system` (
  `SystemCompositionID`   CHAR(36)        NOT NULL            COMMENT 'Khóa chính',
  `Code`                  VARCHAR(255)    NOT NULL            COMMENT 'Mã thành phần hệ thống — unique',
  `Name`                  VARCHAR(255)    NOT NULL            COMMENT 'Tên thành phần hệ thống',
  `ComponentTypeID`       CHAR(36)        NULL                COMMENT 'FK → pa_salary_component_type',
  `Nature`                TINYINT         NOT NULL            COMMENT '1=Thu nhập|2=Khấu trừ|3=Thông tin|4=Khác',
  `TaxType`               TINYINT         NULL                COMMENT '1=Chịu thuế|2=Miễn toàn phần|3=Miễn một phần (chỉ khi Nature=1)',
  `ValueType`             TINYINT         NOT NULL DEFAULT 1  COMMENT '1=Tiền tệ|2=Số|3=Phần trăm',
  `DefaultValue`          DECIMAL(18,4)   NULL                COMMENT 'Giá trị mặc định',
  `NormFormula`           TEXT            NULL                COMMENT 'Công thức định mức (mức trần)',
  `Description`           VARCHAR(1000)   NULL,
  `IsActive`              TINYINT(1)      NOT NULL DEFAULT 1,
  `CreatedBy`             VARCHAR(100)    NULL,
  `CreateDate`            DATETIME        NULL,
  `ModifiedBy`            VARCHAR(100)    NULL,
  `ModifiedDate`          DATETIME        NULL,
  PRIMARY KEY (`SystemCompositionID`),
  UNIQUE KEY `UQ_Code` (`Code`),
  KEY `IDX_ComponentType` (`ComponentTypeID`),
  KEY `IDX_Nature`        (`Nature`),
  CONSTRAINT `FK_system_component_type`
    FOREIGN KEY (`ComponentTypeID`) REFERENCES `pa_salary_component_type` (`ComponentTypeID`)
    ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Danh mục thành phần lương chuẩn của hệ thống';

-- ============================================================
-- PROC: Proc_Insertpa_salary_composition_system
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Insertpa_salary_composition_system`;
delimiter ;;
CREATE PROCEDURE `Proc_Insertpa_salary_composition_system`(
  IN v_SystemCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_ComponentTypeID      CHAR(36),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_ValueType            TINYINT,
  IN v_DefaultValue         DECIMAL(18,4),
  IN v_NormFormula          TEXT,
  IN v_Description          VARCHAR(1000),
  IN v_IsActive             TINYINT(1),
  IN v_CreatedBy            VARCHAR(100),
  IN v_CreateDate           DATETIME
)
BEGIN
  -- BR-05: TaxType chỉ có ý nghĩa khi Nature = 1 (Thu nhập)
  IF v_TaxType IS NOT NULL AND v_Nature <> 1 THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập';
  END IF;

  INSERT INTO pa_salary_composition_system (
    SystemCompositionID, Code, Name, ComponentTypeID, Nature,
    TaxType, ValueType, DefaultValue, NormFormula, Description,
    IsActive, CreatedBy, CreateDate
  ) VALUES (
    v_SystemCompositionID, v_Code, v_Name, v_ComponentTypeID, v_Nature,
    v_TaxType, IFNULL(v_ValueType, 1), v_DefaultValue, v_NormFormula, v_Description,
    IFNULL(v_IsActive, 1), v_CreatedBy, v_CreateDate
  );
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Updatepa_salary_composition_system
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Updatepa_salary_composition_system`;
delimiter ;;
CREATE PROCEDURE `Proc_Updatepa_salary_composition_system`(
  IN v_SystemCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_ComponentTypeID      CHAR(36),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_ValueType            TINYINT,
  IN v_DefaultValue         DECIMAL(18,4),
  IN v_NormFormula          TEXT,
  IN v_Description          VARCHAR(1000),
  IN v_IsActive             TINYINT(1),
  IN v_ModifiedBy           VARCHAR(100),
  IN v_ModifiedDate         DATETIME
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_salary_composition_system WHERE SystemCompositionID = v_SystemCompositionID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Thành phần lương hệ thống không tồn tại';
  END IF;

  -- BR-05: TaxType chỉ có ý nghĩa khi Nature = 1
  IF v_TaxType IS NOT NULL AND v_Nature <> 1 THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập';
  END IF;

  UPDATE pa_salary_composition_system
  SET Code         = v_Code,
      Name         = v_Name,
      ComponentTypeID = v_ComponentTypeID,
      Nature       = v_Nature,
      TaxType      = v_TaxType,
      ValueType    = v_ValueType,
      DefaultValue = v_DefaultValue,
      NormFormula  = v_NormFormula,
      Description  = v_Description,
      IsActive     = v_IsActive,
      ModifiedBy   = v_ModifiedBy,
      ModifiedDate = v_ModifiedDate
  WHERE SystemCompositionID = v_SystemCompositionID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Deletepa_salary_composition_systemById (xóa cứng)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Deletepa_salary_composition_systemById`;
delimiter ;;
CREATE PROCEDURE `Proc_Deletepa_salary_composition_systemById`(
  IN v_SystemCompositionID CHAR(36)
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_salary_composition_system WHERE SystemCompositionID = v_SystemCompositionID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Thành phần lương hệ thống không tồn tại';
  END IF;

  -- Chặn xóa nếu đang được tham chiếu bởi pa_salary_composition
  IF EXISTS (SELECT 1 FROM pa_salary_composition WHERE SystemCompositionID = v_SystemCompositionID AND IsDeleted = 0) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Không thể xóa — đang được sử dụng bởi thành phần lương đơn vị';
  END IF;

  DELETE FROM pa_salary_composition_system WHERE SystemCompositionID = v_SystemCompositionID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_pa_salary_composition_system_FilterPaging (generic paging)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_pa_salary_composition_system_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_pa_salary_composition_system_FilterPaging`(
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
    SET v_orderBy = ' ORDER BY CreateDate DESC';
  END IF;

  SET @v_sql = CONCAT(
    'SELECT * FROM pa_salary_composition_system', v_where, v_orderBy,
    ' LIMIT ', v_offset, ',', v_pageSize
  );
  SET @v_sqlCount = CONCAT('SELECT COUNT(*) AS Total FROM pa_salary_composition_system', v_where);

  PREPARE stmt FROM @v_sql;      EXECUTE stmt;      DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @v_sqlCount; EXECUTE stmt;      DEALLOCATE PREPARE stmt;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_pa_salary_composition_system_Filter (custom filter)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_pa_salary_composition_system_Filter`;
delimiter ;;
CREATE PROCEDURE `Proc_pa_salary_composition_system_Filter`(
  IN v_Search           VARCHAR(255),
  IN v_ComponentTypeID  CHAR(36),
  IN v_Nature           TINYINT,
  IN v_IsActive         TINYINT(1),
  IN v_PageIndex        INT,
  IN v_PageSize         INT
)
BEGIN
  DECLARE v_offset INT;

  IF v_PageIndex < 1 THEN SET v_PageIndex = 1; END IF;
  IF v_PageSize  < 1 THEN SET v_PageSize  = 20; END IF;
  SET v_offset = (v_PageIndex - 1) * v_PageSize;

  SET @sql      = 'SELECT * FROM pa_salary_composition_system WHERE 1=1';
  SET @sqlCount = 'SELECT COUNT(*) AS Total FROM pa_salary_composition_system WHERE 1=1';
  SET @cond     = '';

  IF v_Search IS NOT NULL AND v_Search <> '' THEN
    SET @cond = CONCAT(@cond, ' AND (Code LIKE "%', v_Search, '%" OR Name LIKE "%', v_Search, '%")');
  END IF;
  IF v_ComponentTypeID IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND ComponentTypeID = "', v_ComponentTypeID, '"');
  END IF;
  IF v_Nature IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND Nature = ', v_Nature);
  END IF;
  IF v_IsActive IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND IsActive = ', v_IsActive);
  END IF;

  SET @sql      = CONCAT(@sql,      @cond, ' ORDER BY CreateDate DESC LIMIT ', v_offset, ',', v_PageSize);
  SET @sqlCount = CONCAT(@sqlCount, @cond);

  PREPARE stmt FROM @sql;      EXECUTE stmt;      DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @sqlCount; EXECUTE stmt;      DEALLOCATE PREPARE stmt;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_pa_salary_composition_system_AdvancedFilterPaging
-- Dùng cho POST /AdvancedFilterProc — SP tự build WHERE từ JSON
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_pa_salary_composition_system_AdvancedFilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_pa_salary_composition_system_AdvancedFilterPaging`(
  IN v_pageIndex INT,
  IN v_pageSize  INT,
  IN v_sort      VARCHAR(500),
  IN v_filters   JSON
)
BEGIN
  DECLARE v_offset INT;

  DECLARE v_i            INT          DEFAULT 0;
  DECLARE v_filter_count INT          DEFAULT 0;
  DECLARE v_field        VARCHAR(255) DEFAULT '';
  DECLARE v_op           VARCHAR(50)  DEFAULT '';
  DECLARE v_value        TEXT         DEFAULT NULL;
  DECLARE v_value_to     TEXT         DEFAULT NULL;
  DECLARE v_cond         TEXT         DEFAULT NULL;
  DECLARE v_where        TEXT         DEFAULT '';

  DECLARE v_j        INT  DEFAULT 0;
  DECLARE v_in_count INT  DEFAULT 0;
  DECLARE v_in_list  TEXT DEFAULT '';

  DECLARE v_sort_count INT          DEFAULT 0;
  DECLARE v_sort_i     INT          DEFAULT 1;
  DECLARE v_sort_part  VARCHAR(255) DEFAULT '';
  DECLARE v_sort_field VARCHAR(255) DEFAULT '';
  DECLARE v_sort_dir   VARCHAR(10)  DEFAULT 'ASC';
  DECLARE v_order_by   TEXT         DEFAULT '';

  DECLARE v_sql       TEXT;
  DECLARE v_count_sql TEXT;

  IF v_pageIndex IS NULL OR v_pageIndex < 1 THEN SET v_pageIndex = 1; END IF;
  IF v_pageSize  IS NULL OR v_pageSize  < 1 THEN SET v_pageSize  = 20; END IF;
  SET v_offset = (v_pageIndex - 1) * v_pageSize;

  -- Build WHERE
  IF v_filters IS NOT NULL AND JSON_LENGTH(v_filters) > 0 THEN
    SET v_filter_count = JSON_LENGTH(v_filters);
    WHILE v_i < v_filter_count DO
      SET v_field = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].field')));
      SET v_op    = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].operator')));
      SET v_cond  = NULL;

      SET v_value = IF(
        JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value')) IS NULL OR
        JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))) = 'NULL',
        NULL,
        JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value')))
      );
      SET v_value_to = IF(
        JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo')) IS NULL OR
        JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))) = 'NULL',
        NULL,
        JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo')))
      );

      IF      v_op = 'Eq'          THEN SET v_cond = CONCAT('`', v_field, '` = ',           QUOTE(v_value));
      ELSEIF  v_op = 'Neq'         THEN SET v_cond = CONCAT('`', v_field, '` <> ',          QUOTE(v_value));
      ELSEIF  v_op = 'Contains'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
      ELSEIF  v_op = 'NotContains' THEN SET v_cond = CONCAT('`', v_field, '` NOT LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
      ELSEIF  v_op = 'StartsWith'  THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(', QUOTE(v_value), ', ''%'')');
      ELSEIF  v_op = 'EndsWith'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ')');
      ELSEIF  v_op = 'Empty'       THEN SET v_cond = CONCAT('(`', v_field, '` IS NULL OR `', v_field, '` = '''')');
      ELSEIF  v_op = 'NotEmpty'    THEN SET v_cond = CONCAT('(`', v_field, '` IS NOT NULL AND `', v_field, '` <> '''')');
      ELSEIF  v_op = 'Gt'          THEN SET v_cond = CONCAT('`', v_field, '` > ',           QUOTE(v_value));
      ELSEIF  v_op = 'Lt'          THEN SET v_cond = CONCAT('`', v_field, '` < ',           QUOTE(v_value));
      ELSEIF  v_op = 'Gte'         THEN SET v_cond = CONCAT('`', v_field, '` >= ',          QUOTE(v_value));
      ELSEIF  v_op = 'Lte'         THEN SET v_cond = CONCAT('`', v_field, '` <= ',          QUOTE(v_value));
      ELSEIF  v_op = 'Between'     THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN ',     QUOTE(v_value), ' AND ', QUOTE(v_value_to));
      ELSEIF  v_op = 'NotBetween'  THEN SET v_cond = CONCAT('`', v_field, '` NOT BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
      ELSEIF  v_op = 'Today'       THEN SET v_cond = CONCAT('DATE(`', v_field, '`) = CURDATE()');
      ELSEIF  v_op = 'ThisWeek'    THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN DATE_SUB(CURDATE(), INTERVAL WEEKDAY(CURDATE()) DAY) AND DATE_ADD(DATE_SUB(CURDATE(), INTERVAL WEEKDAY(CURDATE()) DAY), INTERVAL 6 DAY)');
      ELSEIF  v_op = 'ThisMonth'   THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN DATE_FORMAT(CURDATE(), ''%Y-%m-01'') AND LAST_DAY(CURDATE())');
      ELSEIF  v_op = 'ThisYear'    THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN DATE_FORMAT(CURDATE(), ''%Y-01-01'') AND DATE_FORMAT(CURDATE(), ''%Y-12-31'')');
      ELSEIF  v_op = 'LastNDays'   THEN SET v_cond = CONCAT('`', v_field, '` >= DATE_SUB(CURDATE(), INTERVAL ', CAST(CAST(v_value AS UNSIGNED) AS CHAR), ' DAY)');
      ELSEIF  v_op = 'NextNDays'   THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL ', CAST(CAST(v_value AS UNSIGNED) AS CHAR), ' DAY)');
      ELSEIF  v_op = 'In' OR v_op = 'NotIn' THEN
        SET v_in_list = ''; SET v_j = 0; SET v_in_count = 0;
        IF JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values')) IS NOT NULL THEN
          SET v_in_count = JSON_LENGTH(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values')));
          WHILE v_j < v_in_count DO
            IF v_j > 0 THEN SET v_in_list = CONCAT(v_in_list, ', '); END IF;
            SET v_in_list = CONCAT(v_in_list, QUOTE(JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values[', v_j, ']')))));
            SET v_j = v_j + 1;
          END WHILE;
        END IF;
        SET v_cond = IF(v_in_list != '',
          CONCAT('`', v_field, IF(v_op = 'In', '` IN (', '` NOT IN ('), v_in_list, ')'),
          IF(v_op = 'In', '1 = 0', '1 = 1')
        );
      END IF;

      IF v_cond IS NOT NULL THEN
        IF v_where != '' THEN SET v_where = CONCAT(v_where, ' AND '); END IF;
        SET v_where = CONCAT(v_where, v_cond);
      END IF;

      SET v_i = v_i + 1;
    END WHILE;
  END IF;

  -- Build ORDER BY
  IF v_sort IS NOT NULL AND v_sort != '' THEN
    SET v_sort_count = 1 + (LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', '')));
    SET v_sort_i = 1; SET v_order_by = 'ORDER BY ';
    WHILE v_sort_i <= v_sort_count DO
      SET v_sort_part = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(v_sort, ',', v_sort_i), ',', -1));
      IF    LEFT(v_sort_part, 1) = '-' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'DESC';
      ELSEIF LEFT(v_sort_part, 1) = '+' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'ASC';
      ELSE SET v_sort_field = v_sort_part; SET v_sort_dir = 'ASC';
      END IF;
      IF v_sort_i > 1 THEN SET v_order_by = CONCAT(v_order_by, ', '); END IF;
      SET v_order_by = CONCAT(v_order_by, '`', v_sort_field, '` ', v_sort_dir);
      SET v_sort_i = v_sort_i + 1;
    END WHILE;
  ELSE
    SET v_order_by = 'ORDER BY `CreateDate` DESC';
  END IF;

  -- Execute
  SET v_sql       = CONCAT('SELECT * FROM `pa_salary_composition_system`',       IF(v_where != '', CONCAT(' WHERE ', v_where), ''), ' ', v_order_by, ' LIMIT ', v_pageSize, ' OFFSET ', v_offset);
  SET v_count_sql = CONCAT('SELECT COUNT(*) FROM `pa_salary_composition_system`', IF(v_where != '', CONCAT(' WHERE ', v_where), ''));

  PREPARE stmt FROM v_sql;       EXECUTE stmt;       DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM v_count_sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
