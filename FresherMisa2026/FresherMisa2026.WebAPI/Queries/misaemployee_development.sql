/*
 Navicat Premium Dump SQL

 Source Server         : localhost
 Source Server Type    : MySQL
 Source Server Version : 80406 (8.4.6)
 Source Host           : localhost:3306
 Source Schema         : misaemployee_development

 Target Server Type    : MySQL
 Target Server Version : 80406 (8.4.6)
 File Encoding         : 65001

 Date: 20/04/2026 23:08:46
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for department
-- ----------------------------
DROP TABLE IF EXISTS `department`;
CREATE TABLE `department`  (
  `DepartmentID` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Khóa chính phòng ban',
  `DepartmentCode` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Mã phòng ban',
  `DepartmentName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Tên phòng ban',
  `Description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT 'Diễn giải',
  PRIMARY KEY (`DepartmentID`) USING BTREE,
  UNIQUE INDEX `UQ_DepartmentCode`(`DepartmentCode` ASC) USING BTREE
) ENGINE = InnoDB AVG_ROW_LENGTH = 2048 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of department
-- ----------------------------
INSERT INTO `department` VALUES ('32a44a52-e4e4-470a-9db6-dc2cc42d8296', 'string', 'string', 'string');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440010', 'RND', 'Research & Development', 'Nghiên cứu và phát triển sản phẩm');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440011', 'CS', 'Customer Service', 'Chăm sóc khách hàng');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440012', 'SALE', 'Sales', 'Kinh doanh và bán hàng');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440013', 'ADMIN', 'Administration', 'Hành chính tổng hợp');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440014', 'LEGAL', 'Legal', 'Pháp chế doanh nghiệp');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440015', 'DATA', 'Data Engineering', 'Quản lý và phân tích dữ liệu');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440016', 'SEC', 'Security', 'An ninh hệ thống');
INSERT INTO `department` VALUES ('550e8400-e29b-41d4-a716-446655440017', 'SUP', 'Support', 'Hỗ trợ kỹ thuật');

-- ----------------------------
-- Table structure for employee
-- ----------------------------
DROP TABLE IF EXISTS `employee`;
CREATE TABLE `employee`  (
  `EmployeeID` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Khóa chính nhân viên',
  `EmployeeCode` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Mã nhân viên',
  `EmployeeName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Tên nhân viên',
  `Gender` int NULL DEFAULT 0 COMMENT 'Giới tính: 0-Nữ, 1-Nam, 2-Khác',
  `DateOfBirth` date NULL DEFAULT NULL COMMENT 'Ngày sinh',
  `PhoneNumber` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT 'Số điện thoại',
  `Email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT 'Email',
  `Address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT 'Địa chỉ',
  `DepartmentID` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Phòng ban',
  `PositionID` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Chức vụ',
  `Salary` decimal(18, 4) NULL DEFAULT 0.0000 COMMENT 'Lương cơ bản',
  `HireDate` datetime NULL DEFAULT NULL COMMENT 'Ngày vào làm',
  `CreatedDate` datetime NULL DEFAULT NULL COMMENT 'Ngày tạo',
  UNIQUE INDEX `UQ_EmployeeCode`(`EmployeeCode` ASC) USING BTREE,
  INDEX `IDX_Employee_Dept_Pos`(`DepartmentID` ASC, `PositionID` ASC, `EmployeeID` DESC) USING BTREE,
  INDEX `IDX_Employee_PositionID`(`PositionID` ASC) USING BTREE,
  INDEX `IDX_Employee_Dept_Salary`(`DepartmentID` ASC, `Salary` ASC) USING BTREE,
  INDEX `IDX_Employee_Dept_HireDate`(`DepartmentID` ASC, `HireDate` ASC) USING BTREE,
  FULLTEXT INDEX `FT_Employee_Search`(`EmployeeName`, `EmployeeCode`, `Email`, `Address`)
) ENGINE = InnoDB AVG_ROW_LENGTH = 1092 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of employee
-- ----------------------------
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000001', 'EMP001', 'Nguyễn Văn An', 1, '1995-03-15', '0912345678', 'an.nguyen@misa.com', 'Hà Nội', '550e8400-e29b-41d4-a716-446655440010', '11111111-1111-1111-1111-111111111111', 18000000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000002', 'EMP002', 'Trần Thị Mai', 0, '1998-07-20', '0987654321', 'mai.tran@misa.com', 'Hồ Chí Minh', '550e8400-e29b-41d4-a716-446655440010', '22222222-2222-2222-2222-222222222222', 15000000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000003', 'EMP003', 'Phạm Minh Hoàng', 1, '1993-11-05', '0905123456', 'hoang.pham@misa.com', 'Đà Nẵng', '550e8400-e29b-41d4-a716-446655440010', '33333333-3333-3333-3333-333333333333', 35000000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000004', 'EMP004', 'Lê Thị Hương', 0, '1996-02-10', '0934567890', 'huong.le@misa.com', 'Hà Nội', '550e8400-e29b-41d4-a716-446655440013', '44444444-4444-4444-4444-444444444444', 14000000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000006', 'EMP006', 'Đặng Thị Lan', 0, '1994-04-22', '0977112233', 'lan.dang@misa.com', 'HCM', '550e8400-e29b-41d4-a716-446655440011', '55555555-5555-5555-5555-555555555555', 20000000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000007', 'EMP007', 'Hoàng Văn Sơn', 1, '1990-01-30', '0944556677', 'son.hoang@misa.com', 'HCM', '550e8400-e29b-41d4-a716-446655440011', '55555555-5555-5555-5555-555555555555', 22000000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000008', 'EMP008', 'Vũ Thị Thu', 0, '1997-06-12', '0966889900', 'thu.vu@misa.com', 'Hà Nội', '550e8400-e29b-41d4-a716-446655440012', '66666666-6666-6666-6666-666666666666', 17000000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000009', 'EMP009', 'Phan Anh Tú', 1, '1999-12-01', '0922113344', 'tu.phan@misa.com', 'HCM', '550e8400-e29b-41d4-a716-446655440012', '66666666-6666-6666-6666-666666666666', 16500000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000012', 'EMP012', 'Nguyễn Trung Kiên', 1, '1998-05-05', '0912000001', 'kien.nguyen@misa.com', 'Hà Nội', '550e8400-e29b-41d4-a716-446655440010', '11111111-1111-1111-1111-111111111111', 17500000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000013', 'EMP013', 'Trịnh Hà My', 0, '1997-09-09', '0912000002', 'my.trinh@misa.com', 'HCM', '550e8400-e29b-41d4-a716-446655440010', '22222222-2222-2222-2222-222222222222', 15500000.0000, '2026-04-15 09:29:18', '2026-04-15 09:29:18');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000005', 'UPD713713', 'Updated Employee 713713', 0, '1997-05-05', '0911002200', 'updated@test.com', 'HCM', '550e8400-e29b-41d4-a716-446655440010', '11111111-1111-1111-1111-111111111111', 15000000.0000, NULL, '2026-04-16 09:00:00');
INSERT INTO `employee` VALUES ('e0000001-0000-0000-0000-000000000014', 'UPD71371ư3', 'Updated Employee 713713', 0, '1997-05-05', '0911002200', 'updated@test.com', 'HCM', '550e8400-e29b-41d4-a716-446655440010', '11111111-1111-1111-1111-111111111111', 15000000.0000, NULL, '2026-04-16 09:00:00');

-- ----------------------------
-- Table structure for position
-- ----------------------------
DROP TABLE IF EXISTS `position`;
CREATE TABLE `position`  (
  `PositionID` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Khóa chính chức vụ',
  `PositionCode` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Mã chức vụ',
  `PositionName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Tên chức vụ',
  PRIMARY KEY (`PositionID`) USING BTREE,
  UNIQUE INDEX `UQ_PositionCode`(`PositionCode` ASC) USING BTREE
) ENGINE = InnoDB AVG_ROW_LENGTH = 2340 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of position
-- ----------------------------
INSERT INTO `position` VALUES ('00000000-0000-0000-0000-000000000001', 'DEV1', 'Senior Developer');
INSERT INTO `position` VALUES ('0e57e405-ce03-47a4-bc0c-2b536b80ab2c', 'POS278452', 'Position 278452');
INSERT INTO `position` VALUES ('11111111-1111-1111-1111-111111111111', 'DevWeb', 'Developer web');
INSERT INTO `position` VALUES ('22222222-2222-2222-2222-222222222222', 'QA', 'Quality Assurance');
INSERT INTO `position` VALUES ('33333333-3333-3333-3333-333333333333', 'PM', 'Project Manager');
INSERT INTO `position` VALUES ('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'string', 'string');
INSERT INTO `position` VALUES ('44444444-4444-4444-4444-444444444444', 'HR', 'HR Specialist');
INSERT INTO `position` VALUES ('55555555-5555-5555-5555-555555555555', 'ACC', 'Accountant');
INSERT INTO `position` VALUES ('66666666-6666-6666-6666-666666666666', 'MKT', 'Marketing Executive');
INSERT INTO `position` VALUES ('77777777-7777-7777-7777-777777777777', 'OPS', 'Operation Staff');

-- ----------------------------
-- Procedure structure for drop_index_if_exists
-- ----------------------------
DROP PROCEDURE IF EXISTS `drop_index_if_exists`;
delimiter ;;
CREATE PROCEDURE `drop_index_if_exists`(IN p_table VARCHAR(64),
  IN p_index VARCHAR(64))
BEGIN
  IF EXISTS (
    SELECT 1 FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME   = p_table
      AND INDEX_NAME   = p_index
  ) THEN
    SET @_sql = CONCAT('ALTER TABLE `', p_table, '` DROP INDEX `', p_index, '`');
    PREPARE _stmt FROM @_sql;
    EXECUTE _stmt;
    DEALLOCATE PREPARE _stmt;
  END IF;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_DeleteDepartmentById
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_DeleteDepartmentById`;
delimiter ;;
CREATE PROCEDURE `Proc_DeleteDepartmentById`(IN v_DepartmentID char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci)
BEGIN
  DELETE FROM Department WHERE DepartmentID = v_DepartmentID;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_DeleteEmployeeById
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_DeleteEmployeeById`;
delimiter ;;
CREATE PROCEDURE `Proc_DeleteEmployeeById`(IN v_EmployeeID char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci)
BEGIN
  DELETE FROM Employee WHERE EmployeeID = v_EmployeeID;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_DeletePositionById
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_DeletePositionById`;
delimiter ;;
CREATE PROCEDURE `Proc_DeletePositionById`(IN v_PositionID char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci)
BEGIN
  DELETE FROM Position WHERE PositionID = v_PositionID;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_DeleteSeedEmployees
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_DeleteSeedEmployees`;
delimiter ;;
CREATE PROCEDURE `Proc_DeleteSeedEmployees`()
BEGIN
    DECLARE v_batch   INT DEFAULT 1000;
    DECLARE v_deleted INT DEFAULT 0;
    DECLARE v_total   INT DEFAULT 0;

    -- Đếm tổng cần xóa
    SELECT COUNT(*) INTO v_total FROM employee WHERE EmployeeCode LIKE 'SEED%';
    SELECT CONCAT('Tổng cần xóa: ', v_total, ' bản ghi') AS Info;

    -- Xóa theo batch để tránh lock bảng quá lâu
    REPEAT
        DELETE FROM employee
        WHERE EmployeeCode LIKE 'SEED%'
        LIMIT 1000;

        SET v_deleted = v_deleted + ROW_COUNT();
        SELECT CONCAT('Đã xóa: ', v_deleted, ' / ', v_total) AS Progress;
        COMMIT;

    UNTIL ROW_COUNT() = 0 END REPEAT;

    SELECT CONCAT('Hoàn thành! Đã xóa tổng cộng ', v_deleted, ' bản ghi.') AS Result;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_Department_FilterPaging
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_Department_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_Department_FilterPaging`(IN v_pageIndex int,
IN v_pageSize int,
IN v_search varchar(255),
IN v_sort varchar(200),
IN v_searchFields json)
BEGIN

  DECLARE v_offset int;
  DECLARE v_where text DEFAULT ' WHERE 1=1 ';
  DECLARE v_orderBy text DEFAULT '';
  DECLARE v_searchCondition text;

  -- 1️⃣ Paging cơ bản
  IF v_pageIndex < 1 THEN
    SET v_pageIndex = 1;
  END IF;
  IF v_pageSize < 1 THEN
    SET v_pageSize = 20;
  END IF;

  SET v_offset = (v_pageIndex - 1) * v_pageSize;

  -- 2️⃣ Search nhiều field
  IF v_search IS NOT NULL
    AND v_search <> ''
    AND v_searchFields IS NOT NULL THEN

    SELECT
      GROUP_CONCAT(
      CONCAT(
      '`',
      JSON_UNQUOTE(JSON_EXTRACT(v_searchFields, CONCAT('$[', n, ']'))),
      '` LIKE "%', v_search, '%"'
      )
      SEPARATOR ' OR '
      ) INTO v_searchCondition
    FROM (SELECT
        0 n
      UNION
      SELECT
        1
      UNION
      SELECT
        2
      UNION
      SELECT
        3
      UNION
      SELECT
        4) t
    WHERE n < JSON_LENGTH(v_searchFields);

    IF v_searchCondition IS NOT NULL THEN
      SET v_where = CONCAT(v_where, ' AND (', v_searchCondition, ')');
    END IF;

  END IF;

  -- 3️⃣ Sort nhiều cột
  IF v_sort IS NOT NULL
    AND v_sort <> '' THEN

    SELECT
      GROUP_CONCAT(
      CONCAT(
      '`', SUBSTRING(item, 2), '` ',
      IF(LEFT(item, 1) = '-', 'DESC', 'ASC')
      )
      SEPARATOR ', '
      ) INTO v_orderBy
    FROM (SELECT
        TRIM(SUBSTRING_INDEX (SUBSTRING_INDEX (v_sort, ',', n), ',', -1)) item
      FROM (SELECT
          1 n
        UNION
        SELECT
          2
        UNION
        SELECT
          3
        UNION
        SELECT
          4) x
      WHERE n <= 1 + LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', ''))) y;

    IF v_orderBy IS NOT NULL THEN
      SET v_orderBy = CONCAT(' ORDER BY ', v_orderBy);
    END IF;

  END IF;

  -- 4️⃣ Default sort
  IF v_orderBy IS NULL
    OR v_orderBy = '' THEN
    SET v_orderBy = ' ORDER BY DepartmentID DESC';
  END IF;

  -- 5️⃣ Build SQL
  SET @v_sql = CONCAT(
  'SELECT * FROM Department ',
  v_where,
  v_orderBy,
  ' LIMIT ', v_offset, ',', v_pageSize, '; '
  );


  -- 6. SQL count
  SET @v_sqlCount = CONCAT(
  'SELECT COUNT(*) AS Total FROM Department ',
  v_where
  );



  PREPARE stmt1 FROM @v_sql;
  EXECUTE stmt1;
  DEALLOCATE PREPARE stmt1;

  PREPARE stmt FROM @v_sqlCount;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;


END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_Employee_Filter
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_Employee_Filter`;
delimiter ;;
CREATE PROCEDURE `Proc_Employee_Filter`(IN v_DepartmentID char(36),
  IN v_PositionID char(36),
  IN v_SalaryFrom decimal(18,4),
  IN v_SalaryTo decimal(18,4),
  IN v_Gender int,
  IN v_HireDateFrom datetime,
  IN v_HireDateTo datetime)
BEGIN
  SELECT *
  FROM employee
  WHERE 1 = 1
    AND (v_DepartmentID IS NULL OR DepartmentID = v_DepartmentID)
    AND (v_PositionID IS NULL OR PositionID = v_PositionID)
    AND (v_SalaryFrom IS NULL OR Salary >= v_SalaryFrom)
    AND (v_SalaryTo IS NULL OR Salary <= v_SalaryTo)
    AND (v_Gender IS NULL OR Gender = v_Gender)
    AND (v_HireDateFrom IS NULL OR HireDate >= v_HireDateFrom)
    AND (v_HireDateTo IS NULL OR HireDate <= v_HireDateTo)
  ORDER BY EmployeeID DESC;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_Employee_FilterPaging
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_Employee_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_Employee_FilterPaging`(IN v_pageIndex int,
IN v_pageSize int,
IN v_search varchar(255),
IN v_sort varchar(200),
IN v_searchFields json)
BEGIN
  DECLARE v_offset int;
  DECLARE v_where text DEFAULT ' WHERE 1=1 ';
  DECLARE v_orderBy text DEFAULT '';
  DECLARE v_searchCondition text;

  -- 1. Paging
  IF v_pageIndex < 1 THEN
    SET v_pageIndex = 1;
  END IF;
  IF v_pageSize < 1 THEN
    SET v_pageSize = 20;
  END IF;

  SET v_offset = (v_pageIndex - 1) * v_pageSize;

  -- 2. Search multiple fields
  IF v_search IS NOT NULL
    AND v_search <> ''
    AND v_searchFields IS NOT NULL THEN

    SELECT
      GROUP_CONCAT(
      CONCAT(
      '`',
      JSON_UNQUOTE(JSON_EXTRACT(v_searchFields, CONCAT('$[', n, ']'))),
      '` LIKE "%', v_search, '%"'
      )
      SEPARATOR ' OR '
      ) INTO v_searchCondition
    FROM (SELECT
        0 n
      UNION
      SELECT
        1
      UNION
      SELECT
        2
      UNION
      SELECT
        3
      UNION
      SELECT
        4
      UNION
      SELECT
        5
      UNION
      SELECT
        6
      UNION
      SELECT
        7
      UNION
      SELECT
        8
      UNION
      SELECT
        9) t
    WHERE n < JSON_LENGTH(v_searchFields);

    IF v_searchCondition IS NOT NULL THEN
      SET v_where = CONCAT(v_where, ' AND (', v_searchCondition, ')');
    END IF;
  END IF;

  -- 3. Sort multiple columns
  IF v_sort IS NOT NULL
    AND v_sort <> '' THEN
    SELECT
      GROUP_CONCAT(
      CONCAT(
      '`', SUBSTRING(item, 2), '` ',
      IF(LEFT(item, 1) = '-', 'DESC', 'ASC')
      )
      SEPARATOR ', '
      ) INTO v_orderBy
    FROM (SELECT
        TRIM(SUBSTRING_INDEX (SUBSTRING_INDEX (v_sort, ',', n), ',', -1)) item
      FROM (SELECT
          1 n
        UNION
        SELECT
          2
        UNION
        SELECT
          3
        UNION
        SELECT
          4
        UNION
        SELECT
          5) x
      WHERE n <= 1 + LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', ''))) y;

    IF v_orderBy IS NOT NULL THEN
      SET v_orderBy = CONCAT(' ORDER BY ', v_orderBy);
    END IF;
  END IF;

  -- 4. Default sort
  IF v_orderBy IS NULL
    OR v_orderBy = '' THEN
    SET v_orderBy = ' ORDER BY EmployeeID DESC';
  END IF;

  -- 5. Build SQL
  SET @v_sql = CONCAT(
  'SELECT * FROM Employee ',
  v_where,
  v_orderBy,
  ' LIMIT ', v_offset, ',', v_pageSize, ';'
  );

  -- 6. SQL count
  SET @v_sqlCount = CONCAT(
  'SELECT COUNT(*) AS Total FROM Employee ',
  v_where
  );

  PREPARE stmt1 FROM @v_sql;
  EXECUTE stmt1;
  DEALLOCATE PREPARE stmt1;

  PREPARE stmt FROM @v_sqlCount;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_Employee_Filter_Paging
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_Employee_Filter_Paging`;
delimiter ;;
CREATE PROCEDURE `Proc_Employee_Filter_Paging`(IN  v_DepartmentID  char(36),
  IN  v_PositionID    char(36),
  IN  v_SalaryFrom    decimal(18,4),
  IN  v_SalaryTo      decimal(18,4),
  IN  v_Gender        int,
  IN  v_HireDateFrom  datetime,
  IN  v_HireDateTo    datetime,
  IN  v_PageSize      int,
  IN  v_PageIndex     int,
  OUT v_Total         bigint)
BEGIN
  DECLARE v_offset INT;
  DECLARE v_where  TEXT DEFAULT ' WHERE 1=1 ';

  IF v_PageIndex < 1 THEN SET v_PageIndex = 1; END IF;
  IF v_PageSize  < 1 THEN SET v_PageSize  = 10; END IF;
  SET v_offset = (v_PageIndex - 1) * v_PageSize;

  IF v_DepartmentID IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND DepartmentID = ', QUOTE(v_DepartmentID));
  END IF;
  IF v_PositionID IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND PositionID = ', QUOTE(v_PositionID));
  END IF;
  IF v_SalaryFrom IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND Salary >= ', v_SalaryFrom);
  END IF;
  IF v_SalaryTo IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND Salary <= ', v_SalaryTo);
  END IF;
  IF v_Gender IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND Gender = ', v_Gender);
  END IF;
  IF v_HireDateFrom IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND HireDate >= ', QUOTE(v_HireDateFrom));
  END IF;
  IF v_HireDateTo IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND HireDate <= ', QUOTE(v_HireDateTo));
  END IF;

  SET @v_sqlCount = CONCAT('SELECT COUNT(*) INTO @_total FROM employee', v_where);
  PREPARE stmtCount FROM @v_sqlCount;
  EXECUTE stmtCount;
  DEALLOCATE PREPARE stmtCount;
  SET v_Total = @_total;

  SET @v_sqlData = CONCAT(
    'SELECT * FROM employee',
    v_where,
    ' ORDER BY EmployeeID DESC',
    ' LIMIT ', v_PageSize, ' OFFSET ', v_offset
  );
  PREPARE stmtData FROM @v_sqlData;
  EXECUTE stmtData;
  DEALLOCATE PREPARE stmtData;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_InsertDepartment
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_InsertDepartment`;
delimiter ;;
CREATE PROCEDURE `Proc_InsertDepartment`(IN v_DepartmentID char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_DepartmentCode varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_DepartmentName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_Description varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci)
BEGIN
  IF EXISTS (SELECT 1 FROM department WHERE DepartmentCode = v_DepartmentCode) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'DepartmentCode đã tồn tại';
  ELSE
    INSERT INTO department (DepartmentID, DepartmentCode, DepartmentName, Description)
    VALUES (v_DepartmentID, v_DepartmentCode, v_DepartmentName, v_Description);
  END IF;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_InsertEmployee
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_InsertEmployee`;
delimiter ;;
CREATE PROCEDURE `Proc_InsertEmployee`(IN v_EmployeeID char(36),
  IN v_EmployeeCode varchar(20),
  IN v_EmployeeName varchar(100),
  IN v_Gender int,
  IN v_DateOfBirth date,
  IN v_PhoneNumber varchar(50),
  IN v_Email varchar(100),
  IN v_Address varchar(255),
  IN v_DepartmentID char(36),
  IN v_PositionID char(36),
  IN v_Salary decimal(18,4),
  IN v_HireDate datetime,
  IN v_CreatedDate datetime)
BEGIN
  IF EXISTS (SELECT 1 FROM employee WHERE EmployeeCode = v_EmployeeCode) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'EmployeeCode đã tồn tại';
  ELSE
    INSERT INTO employee (
      EmployeeID, EmployeeCode, EmployeeName, Gender, DateOfBirth,
      PhoneNumber, Email, Address, DepartmentID, PositionID, Salary, HireDate, CreatedDate
    ) VALUES (
      v_EmployeeID, v_EmployeeCode, v_EmployeeName, v_Gender, v_DateOfBirth,
      v_PhoneNumber, v_Email, v_Address, v_DepartmentID, v_PositionID, v_Salary, v_HireDate, v_CreatedDate
    );
  END IF;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_InsertPosition
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_InsertPosition`;
delimiter ;;
CREATE PROCEDURE `Proc_InsertPosition`(IN v_PositionID char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_PositionCode varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_PositionName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci)
BEGIN
  IF EXISTS (SELECT 1 FROM position WHERE PositionCode = v_PositionCode) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'PositionCode đã tồn tại';
  ELSE
    INSERT INTO position (PositionID, PositionCode, PositionName)
    VALUES (v_PositionID, v_PositionCode, v_PositionName);
  END IF;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_Position_FilterPaging
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_Position_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_Position_FilterPaging`(IN v_pageIndex int,
IN v_pageSize int,
IN v_search varchar(255),
IN v_sort varchar(200),
IN v_searchFields json)
BEGIN
  DECLARE v_offset int;
  DECLARE v_where text DEFAULT ' WHERE 1=1 ';
  DECLARE v_orderBy text DEFAULT '';
  DECLARE v_searchCondition text;

  -- 1. Paging
  IF v_pageIndex < 1 THEN
    SET v_pageIndex = 1;
  END IF;
  IF v_pageSize < 1 THEN
    SET v_pageSize = 20;
  END IF;

  SET v_offset = (v_pageIndex - 1) * v_pageSize;

  -- 2. Search multiple fields
  IF v_search IS NOT NULL
    AND v_search <> ''
    AND v_searchFields IS NOT NULL THEN

    SELECT
      GROUP_CONCAT(
      CONCAT(
      '`',
      JSON_UNQUOTE(JSON_EXTRACT(v_searchFields, CONCAT('$[', n, ']'))),
      '` LIKE "%', v_search, '%"'
      )
      SEPARATOR ' OR '
      ) INTO v_searchCondition
    FROM (SELECT
        0 n
      UNION
      SELECT
        1
      UNION
      SELECT
        2
      UNION
      SELECT
        3
      UNION
      SELECT
        4) t
    WHERE n < JSON_LENGTH(v_searchFields);

    IF v_searchCondition IS NOT NULL THEN
      SET v_where = CONCAT(v_where, ' AND (', v_searchCondition, ')');
    END IF;
  END IF;

  -- 3. Sort multiple columns
  IF v_sort IS NOT NULL
    AND v_sort <> '' THEN
    SELECT
      GROUP_CONCAT(
      CONCAT(
      '`', SUBSTRING(item, 2), '` ',
      IF(LEFT(item, 1) = '-', 'DESC', 'ASC')
      )
      SEPARATOR ', '
      ) INTO v_orderBy
    FROM (SELECT
        TRIM(SUBSTRING_INDEX (SUBSTRING_INDEX (v_sort, ',', n), ',', -1)) item
      FROM (SELECT
          1 n
        UNION
        SELECT
          2
        UNION
        SELECT
          3
        UNION
        SELECT
          4) x
      WHERE n <= 1 + LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', ''))) y;

    IF v_orderBy IS NOT NULL THEN
      SET v_orderBy = CONCAT(' ORDER BY ', v_orderBy);
    END IF;
  END IF;

  -- 4. Default sort
  IF v_orderBy IS NULL
    OR v_orderBy = '' THEN
    SET v_orderBy = ' ORDER BY PositionID DESC';
  END IF;

  -- 5. Build SQL
  SET @v_sql = CONCAT(
  'SELECT * FROM Position ',
  v_where,
  v_orderBy,
  ' LIMIT ', v_offset, ',', v_pageSize, ';'
  );

  -- 6. SQL count
  SET @v_sqlCount = CONCAT(
  'SELECT COUNT(*) AS Total FROM Position ',
  v_where
  );

  PREPARE stmt1 FROM @v_sql;
  EXECUTE stmt1;
  DEALLOCATE PREPARE stmt1;

  PREPARE stmt FROM @v_sqlCount;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_SeedEmployees
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_SeedEmployees`;
delimiter ;;
CREATE PROCEDURE `Proc_SeedEmployees`(IN p_count INT)
BEGIN
    DECLARE i          INT DEFAULT 1;
    DECLARE v_uuid     CHAR(36);
    DECLARE v_code     VARCHAR(20);
    DECLARE v_name     VARCHAR(100);
    DECLARE v_gender   INT;
    DECLARE v_dob      DATE;
    DECLARE v_phone    VARCHAR(50);
    DECLARE v_email    VARCHAR(100);
    DECLARE v_address  VARCHAR(255);
    DECLARE v_deptID   CHAR(36);
    DECLARE v_posID    CHAR(36);
    DECLARE v_salary   DECIMAL(18,4);
    DECLARE v_hire     DATETIME;
    DECLARE v_created  DATETIME;

    -- Mảng giả: danh sách DepartmentID có sẵn
    -- (8 phòng ban thực tế trong DB)
    DECLARE dept1 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440010';
    DECLARE dept2 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440011';
    DECLARE dept3 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440012';
    DECLARE dept4 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440013';
    DECLARE dept5 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440014';
    DECLARE dept6 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440015';
    DECLARE dept7 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440016';
    DECLARE dept8 CHAR(36) DEFAULT '550e8400-e29b-41d4-a716-446655440017';

    -- Mảng giả: danh sách PositionID có sẵn
    DECLARE pos1 CHAR(36) DEFAULT '11111111-1111-1111-1111-111111111111';
    DECLARE pos2 CHAR(36) DEFAULT '22222222-2222-2222-2222-222222222222';
    DECLARE pos3 CHAR(36) DEFAULT '33333333-3333-3333-3333-333333333333';
    DECLARE pos4 CHAR(36) DEFAULT '44444444-4444-4444-4444-444444444444';
    DECLARE pos5 CHAR(36) DEFAULT '55555555-5555-5555-5555-555555555555';
    DECLARE pos6 CHAR(36) DEFAULT '66666666-6666-6666-6666-666666666666';
    DECLARE pos7 CHAR(36) DEFAULT '77777777-7777-7777-7777-777777777777';

    main_loop: WHILE i <= p_count DO

        -- UUID ngẫu nhiên
        SET v_uuid = UUID();

        -- Mã nhân viên: SEED + số thứ tự 7 chữ số
        SET v_code = CONCAT('SEED', LPAD(i, 7, '0'));

        -- Tên: kết hợp họ + tên theo nhóm i mod
        SET v_name = CONCAT(
            ELT(1 + (i MOD 20),
                'Nguyễn','Trần','Lê','Phạm','Hoàng','Huỳnh','Phan','Vũ',
                'Võ','Đặng','Bùi','Đỗ','Hồ','Ngô','Dương','Lý',
                'Đinh','Mai','Trịnh','Tô'
            ), ' ',
            ELT(1 + (FLOOR(i/20) MOD 30),
                'Văn An','Thị Mai','Minh Hoàng','Thị Hương','Quốc Bảo',
                'Thị Lan','Văn Sơn','Thị Thu','Anh Tú','Trung Kiên',
                'Hà My','Minh Tuấn','Thanh Hà','Đức Thành','Thị Ngọc',
                'Hoài Nam','Thị Phương','Quang Huy','Thị Linh','Đình Long',
                'Bích Ngọc','Hữu Phúc','Thị Trang','Công Danh','Thị Yến',
                'Quốc Hùng','Thị Kim','Minh Châu','Thị Diễm','Văn Lộc'
            )
        );

        -- Giới tính: 0-Nữ, 1-Nam, 2-Khác
        SET v_gender = i MOD 3;

        -- Ngày sinh: từ 1975 đến 2000
        SET v_dob = DATE_ADD('1975-01-01', INTERVAL FLOOR(RAND() * 9500) DAY);

        -- Số điện thoại
        SET v_phone = CONCAT('09', LPAD(FLOOR(RAND() * 100000000), 8, '0'));

        -- Email
        SET v_email = CONCAT('seed', i, '@perftest.local');

        -- Địa chỉ
        SET v_address = ELT(1 + (i MOD 5), 'Hà Nội','Hồ Chí Minh','Đà Nẵng','Cần Thơ','Hải Phòng');

        -- DepartmentID (xoay vòng 8 phòng ban)
        SET v_deptID = ELT(1 + (i MOD 8), dept1,dept2,dept3,dept4,dept5,dept6,dept7,dept8);

        -- PositionID (xoay vòng 7 chức vụ)
        SET v_posID = ELT(1 + (i MOD 7), pos1,pos2,pos3,pos4,pos5,pos6,pos7);

        -- Lương: từ 8 triệu đến 50 triệu
        SET v_salary = (8000000 + FLOOR(RAND() * 42000000));

        -- Ngày vào làm: từ 2015 đến 2025
        SET v_hire = DATE_ADD('2015-01-01', INTERVAL FLOOR(RAND() * 3650) DAY);

        -- Ngày tạo
        SET v_created = NOW();

        INSERT INTO `employee`
            (EmployeeID, EmployeeCode, EmployeeName, Gender,
             DateOfBirth, PhoneNumber, Email, Address,
             DepartmentID, PositionID, Salary, HireDate, CreatedDate)
        VALUES
            (v_uuid, v_code, v_name, v_gender,
             v_dob, v_phone, v_email, v_address,
             v_deptID, v_posID, v_salary, v_hire, v_created);

        -- Commit theo batch 1000 dòng để tránh lock lớn
        IF (i MOD 1000 = 0) THEN
            COMMIT;
        END IF;

        SET i = i + 1;
    END WHILE;

    COMMIT; -- Commit phần còn lại
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_UpdateDepartment
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_UpdateDepartment`;
delimiter ;;
CREATE PROCEDURE `Proc_UpdateDepartment`(IN v_DepartmentID char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_DepartmentCode varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_DepartmentName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_Description varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM department WHERE DepartmentID = v_DepartmentID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Department không tồn tại';
  END IF;

  IF EXISTS (SELECT 1 FROM department WHERE DepartmentCode = v_DepartmentCode AND DepartmentID <> v_DepartmentID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'DepartmentCode đã tồn tại';
  END IF;

  UPDATE department
  SET DepartmentCode = v_DepartmentCode,
      DepartmentName = v_DepartmentName,
      Description = v_Description
  WHERE DepartmentID = v_DepartmentID;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_UpdateEmployee
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_UpdateEmployee`;
delimiter ;;
CREATE PROCEDURE `Proc_UpdateEmployee`(IN v_EmployeeID char(36),
  IN v_EmployeeCode varchar(20),
  IN v_EmployeeName varchar(100),
  IN v_Gender int,
  IN v_DateOfBirth date,
  IN v_PhoneNumber varchar(50),
  IN v_Email varchar(100),
  IN v_Address varchar(255),
  IN v_DepartmentID char(36),
  IN v_PositionID char(36),
  IN v_Salary decimal(18,4),
  IN v_HireDate datetime,
  IN v_CreatedDate datetime)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM employee WHERE EmployeeID = v_EmployeeID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Employee không tồn tại';
  END IF;

  IF EXISTS (SELECT 1 FROM employee WHERE EmployeeCode = v_EmployeeCode AND EmployeeID <> v_EmployeeID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'EmployeeCode đã tồn tại';
  END IF;

  UPDATE employee
  SET EmployeeCode = v_EmployeeCode,
      EmployeeName = v_EmployeeName,
      Gender = v_Gender,
      DateOfBirth = v_DateOfBirth,
      PhoneNumber = v_PhoneNumber,
      Email = v_Email,
      Address = v_Address,
      DepartmentID = v_DepartmentID,
      PositionID = v_PositionID,
      Salary = v_Salary,
      HireDate = v_HireDate,
      CreatedDate = v_CreatedDate
  WHERE EmployeeID = v_EmployeeID;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for Proc_UpdatePosition
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_UpdatePosition`;
delimiter ;;
CREATE PROCEDURE `Proc_UpdatePosition`(IN v_PositionID char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_PositionCode varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  IN v_PositionName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM position WHERE PositionID = v_PositionID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Position không tồn tại';
  END IF;

  IF EXISTS (SELECT 1 FROM position WHERE PositionCode = v_PositionCode AND PositionID <> v_PositionID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'PositionCode đã tồn tại';
  END IF;

  UPDATE position
  SET PositionCode = v_PositionCode,
      PositionName = v_PositionName
  WHERE PositionID = v_PositionID;
END
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
