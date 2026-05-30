-- ============================================================
-- AMIS TIEN LUONG - GRID CONFIG
-- Database: amis_tien_luong
-- Chạy: mysql -u root -p amis_tien_luong < grid_config_migration.sql
--
-- Tên proc theo convention BaseRepository: Proc_{tableName}_*
-- tableName = "pa_grid_config" (theo [ConfigTable])
-- hasDeletedColumn = false → xóa cứng
-- Composite unique: (UserID, GridCode, ColumnKey)
-- ============================================================

SET NAMES utf8mb4;

USE amis_tien_luong;

-- ============================================================
-- TABLE: pa_grid_config
-- ============================================================
DROP TABLE IF EXISTS `pa_grid_config`;
CREATE TABLE `pa_grid_config` (
  `GridConfigID`  CHAR(36)        NOT NULL            COMMENT 'Khóa chính',
  `UserID`        VARCHAR(255)    NOT NULL            COMMENT 'ID người dùng sở hữu cấu hình',
  `GridCode`      VARCHAR(100)    NOT NULL            COMMENT 'Mã lưới, ví dụ: SALARY_COMPOSITION_LIST',
  `ColumnKey`     VARCHAR(100)    NOT NULL            COMMENT 'Mã cột, ví dụ: code, name, status',
  `Caption`       VARCHAR(255)    NULL                COMMENT 'Tên hiển thị cột (override)',
  `OrderIndex`    INT             NOT NULL DEFAULT 0  COMMENT 'Thứ tự cột',
  `Width`         INT             NULL                COMMENT 'Độ rộng cột (px)',
  `IsPinned`      TINYINT(1)      NOT NULL DEFAULT 0  COMMENT '1=ghim cột',
  `PinPosition`   INT             NULL                COMMENT '1=ghim trái|2=ghim phải',
  `IsVisible`     TINYINT(1)      NOT NULL DEFAULT 1  COMMENT '1=hiển thị|0=ẩn',
  `CreatedBy`     VARCHAR(100)    NULL,
  `CreateDate`    DATETIME        NULL,
  `ModifiedBy`    VARCHAR(100)    NULL,
  `ModifiedDate`  DATETIME        NULL,
  PRIMARY KEY (`GridConfigID`),
  UNIQUE KEY `UQ_User_Grid_Column` (`UserID`, `GridCode`, `ColumnKey`),
  KEY `IDX_UserGrid` (`UserID`, `GridCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Cấu hình cột lưới dữ liệu theo từng người dùng';

-- ============================================================
-- PROC: Proc_Insertpa_grid_config
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Insertpa_grid_config`;
delimiter ;;
CREATE PROCEDURE `Proc_Insertpa_grid_config`(
  IN v_GridConfigID   CHAR(36),
  IN v_UserID         VARCHAR(255),
  IN v_GridCode       VARCHAR(100),
  IN v_ColumnKey      VARCHAR(100),
  IN v_Caption        VARCHAR(255),
  IN v_OrderIndex     INT,
  IN v_Width          INT,
  IN v_IsPinned       TINYINT(1),
  IN v_PinPosition    INT,
  IN v_IsVisible      TINYINT(1),
  IN v_CreatedBy      VARCHAR(100),
  IN v_CreateDate     DATETIME
)
BEGIN
  INSERT INTO pa_grid_config (
    GridConfigID, UserID, GridCode, ColumnKey, Caption,
    OrderIndex, Width, IsPinned, PinPosition, IsVisible,
    CreatedBy, CreateDate
  ) VALUES (
    v_GridConfigID, v_UserID, v_GridCode, v_ColumnKey, v_Caption,
    IFNULL(v_OrderIndex, 0), v_Width, IFNULL(v_IsPinned, 0), v_PinPosition, IFNULL(v_IsVisible, 1),
    v_CreatedBy, v_CreateDate
  );
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Updatepa_grid_config
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Updatepa_grid_config`;
delimiter ;;
CREATE PROCEDURE `Proc_Updatepa_grid_config`(
  IN v_GridConfigID   CHAR(36),
  IN v_UserID         VARCHAR(255),
  IN v_GridCode       VARCHAR(100),
  IN v_ColumnKey      VARCHAR(100),
  IN v_Caption        VARCHAR(255),
  IN v_OrderIndex     INT,
  IN v_Width          INT,
  IN v_IsPinned       TINYINT(1),
  IN v_PinPosition    INT,
  IN v_IsVisible      TINYINT(1),
  IN v_ModifiedBy     VARCHAR(100),
  IN v_ModifiedDate   DATETIME
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_grid_config WHERE GridConfigID = v_GridConfigID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Cấu hình lưới không tồn tại';
  END IF;

  UPDATE pa_grid_config
  SET UserID       = v_UserID,
      GridCode     = v_GridCode,
      ColumnKey    = v_ColumnKey,
      Caption      = v_Caption,
      OrderIndex   = v_OrderIndex,
      Width        = v_Width,
      IsPinned     = v_IsPinned,
      PinPosition  = v_PinPosition,
      IsVisible    = v_IsVisible,
      ModifiedBy   = v_ModifiedBy,
      ModifiedDate = v_ModifiedDate
  WHERE GridConfigID = v_GridConfigID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_Deletepa_grid_configById (xóa cứng)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Deletepa_grid_configById`;
delimiter ;;
CREATE PROCEDURE `Proc_Deletepa_grid_configById`(
  IN v_GridConfigID CHAR(36)
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_grid_config WHERE GridConfigID = v_GridConfigID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Cấu hình lưới không tồn tại';
  END IF;

  DELETE FROM pa_grid_config WHERE GridConfigID = v_GridConfigID;
END
;;
delimiter ;

-- ============================================================
-- PROC: Proc_pa_grid_config_FilterPaging (generic paging)
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_pa_grid_config_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_pa_grid_config_FilterPaging`(
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
    SET v_orderBy = ' ORDER BY UserID ASC, GridCode ASC, OrderIndex ASC';
  END IF;

  SET @v_sql = CONCAT(
    'SELECT * FROM pa_grid_config', v_where, v_orderBy,
    ' LIMIT ', v_offset, ',', v_pageSize
  );
  SET @v_sqlCount = CONCAT('SELECT COUNT(*) AS Total FROM pa_grid_config', v_where);

  PREPARE stmt FROM @v_sql;      EXECUTE stmt;      DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @v_sqlCount; EXECUTE stmt;      DEALLOCATE PREPARE stmt;
END
;;
delimiter ;
