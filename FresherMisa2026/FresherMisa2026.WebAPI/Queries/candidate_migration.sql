-- ============================================================
-- Migration: Candidate feature
-- Created By: ntdo (09/05/2026)
-- ============================================================

SET NAMES utf8mb4;

-- ----------------------------
-- Table structure for candidate
-- ----------------------------
DROP TABLE IF EXISTS `candidate`;
CREATE TABLE `candidate` (
  `CandidateID`      char(36)      NOT NULL COMMENT 'Khóa chính ứng viên',
  `CVFile`           varchar(255)  NULL DEFAULT NULL COMMENT 'Đường dẫn file CV',
  `Avatar`           varchar(255)  NULL DEFAULT NULL COMMENT 'Ảnh đại diện',
  `FullName`         varchar(100)  NOT NULL COMMENT 'Họ và tên',
  `DateOfBirth`      date          NULL DEFAULT NULL COMMENT 'Ngày sinh',
  `Gender`           varchar(10)   NULL DEFAULT NULL COMMENT 'Giới tính: Nam/Nữ/Khác',
  `City`             varchar(100)  NULL DEFAULT NULL COMMENT 'Thành phố',
  `PhoneNumber`      varchar(20)   NULL DEFAULT NULL COMMENT 'Số điện thoại',
  `Email`            varchar(100)  NULL DEFAULT NULL COMMENT 'Email',
  `Country`          varchar(100)  NULL DEFAULT NULL COMMENT 'Quốc gia',
  `Province`         varchar(100)  NULL DEFAULT NULL COMMENT 'Tỉnh/Thành phố',
  `Ward`             varchar(100)  NULL DEFAULT NULL COMMENT 'Phường/Xã',
  `Address`          varchar(255)  NULL DEFAULT NULL COMMENT 'Địa chỉ',
  `Level`            varchar(50)   NULL DEFAULT NULL COMMENT 'Cấp độ: Junior/Mid/Senior',
  `EducationPlace`   varchar(255)  NULL DEFAULT NULL COMMENT 'Nơi học',
  `Major`            varchar(100)  NULL DEFAULT NULL COMMENT 'Chuyên ngành',
  `HiringDate`       date          NULL DEFAULT NULL COMMENT 'Ngày ứng tuyển',
  `CandidateSource`  varchar(100)  NULL DEFAULT NULL COMMENT 'Nguồn ứng viên',
  `HRInCharge`       varchar(100)  NULL DEFAULT NULL COMMENT 'HR phụ trách',
  `Collaborator`     varchar(100)  NULL DEFAULT NULL COMMENT 'Cộng tác viên',
  `IsReferenceAdded` tinyint(1)    NOT NULL DEFAULT 0 COMMENT 'Đã thêm tham chiếu',
  `LastCompany`      varchar(255)  NULL DEFAULT NULL COMMENT 'Công ty cũ',
  `WorkCompany`      varchar(255)  NULL DEFAULT NULL COMMENT 'Công ty đang làm',
  `WorkStartDate`    date          NULL DEFAULT NULL COMMENT 'Ngày bắt đầu làm việc',
  `WorkEndDate`      date          NULL DEFAULT NULL COMMENT 'Ngày kết thúc làm việc',
  `WorkPosition`     varchar(100)  NULL DEFAULT NULL COMMENT 'Vị trí làm việc',
  `WorkDescription`  text          NULL COMMENT 'Mô tả công việc',
  `HiringCampaign`   varchar(100)  NULL DEFAULT NULL COMMENT 'Chiến dịch tuyển dụng',
  `HiringRound`      varchar(50)   NULL DEFAULT NULL COMMENT 'Vòng phỏng vấn',
  `Rating`           varchar(10)   NULL DEFAULT NULL COMMENT 'Đánh giá',
  `JobPosition`      varchar(100)  NULL DEFAULT NULL COMMENT 'Vị trí ứng tuyển',
  `IsEmployee`       tinyint(1)    NOT NULL DEFAULT 0 COMMENT 'Đã là nhân viên',
  `Department`       varchar(100)  NULL DEFAULT NULL COMMENT 'Phòng ban',
  PRIMARY KEY (`CandidateID`),
  INDEX `IDX_Candidate_FullName` (`FullName` ASC),
  INDEX `IDX_Candidate_HiringDate` (`HiringDate` ASC),
  INDEX `IDX_Candidate_JobPosition_Level` (`JobPosition` ASC, `Level` ASC),
  FULLTEXT INDEX `FT_Candidate_Search` (`FullName`, `Email`, `PhoneNumber`, `JobPosition`)
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;


-- ----------------------------
-- Procedure: Proc_InsertCandidate
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_InsertCandidate`;
delimiter ;;
CREATE PROCEDURE `Proc_InsertCandidate`(
  IN v_CandidateID      char(36),
  IN v_CVFile           varchar(255),
  IN v_Avatar           varchar(255),
  IN v_FullName         varchar(100),
  IN v_DateOfBirth      date,
  IN v_Gender           varchar(10),
  IN v_City             varchar(100),
  IN v_PhoneNumber      varchar(20),
  IN v_Email            varchar(100),
  IN v_Country          varchar(100),
  IN v_Province         varchar(100),
  IN v_Ward             varchar(100),
  IN v_Address          varchar(255),
  IN v_Level            varchar(50),
  IN v_EducationPlace   varchar(255),
  IN v_Major            varchar(100),
  IN v_HiringDate       date,
  IN v_CandidateSource  varchar(100),
  IN v_HRInCharge       varchar(100),
  IN v_Collaborator     varchar(100),
  IN v_IsReferenceAdded tinyint(1),
  IN v_LastCompany      varchar(255),
  IN v_WorkCompany      varchar(255),
  IN v_WorkStartDate    date,
  IN v_WorkEndDate      date,
  IN v_WorkPosition     varchar(100),
  IN v_WorkDescription  text,
  IN v_HiringCampaign   varchar(100),
  IN v_HiringRound      varchar(50),
  IN v_Rating           varchar(10),
  IN v_JobPosition      varchar(100),
  IN v_IsEmployee       tinyint(1),
  IN v_Department       varchar(100)
)
BEGIN
  INSERT INTO candidate (
    CandidateID, CVFile, Avatar, FullName, DateOfBirth, Gender, City,
    PhoneNumber, Email, Country, Province, Ward, Address, Level,
    EducationPlace, Major, HiringDate, CandidateSource, HRInCharge,
    Collaborator, IsReferenceAdded, LastCompany, WorkCompany,
    WorkStartDate, WorkEndDate, WorkPosition, WorkDescription,
    HiringCampaign, HiringRound, Rating, JobPosition, IsEmployee, Department
  ) VALUES (
    v_CandidateID, v_CVFile, v_Avatar, v_FullName, v_DateOfBirth, v_Gender, v_City,
    v_PhoneNumber, v_Email, v_Country, v_Province, v_Ward, v_Address, v_Level,
    v_EducationPlace, v_Major, v_HiringDate, v_CandidateSource, v_HRInCharge,
    v_Collaborator, v_IsReferenceAdded, v_LastCompany, v_WorkCompany,
    v_WorkStartDate, v_WorkEndDate, v_WorkPosition, v_WorkDescription,
    v_HiringCampaign, v_HiringRound, v_Rating, v_JobPosition, v_IsEmployee, v_Department
  );
END
;;
delimiter ;


-- ----------------------------
-- Procedure: Proc_UpdateCandidate
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_UpdateCandidate`;
delimiter ;;
CREATE PROCEDURE `Proc_UpdateCandidate`(
  IN v_CandidateID      char(36),
  IN v_CVFile           varchar(255),
  IN v_Avatar           varchar(255),
  IN v_FullName         varchar(100),
  IN v_DateOfBirth      date,
  IN v_Gender           varchar(10),
  IN v_City             varchar(100),
  IN v_PhoneNumber      varchar(20),
  IN v_Email            varchar(100),
  IN v_Country          varchar(100),
  IN v_Province         varchar(100),
  IN v_Ward             varchar(100),
  IN v_Address          varchar(255),
  IN v_Level            varchar(50),
  IN v_EducationPlace   varchar(255),
  IN v_Major            varchar(100),
  IN v_HiringDate       date,
  IN v_CandidateSource  varchar(100),
  IN v_HRInCharge       varchar(100),
  IN v_Collaborator     varchar(100),
  IN v_IsReferenceAdded tinyint(1),
  IN v_LastCompany      varchar(255),
  IN v_WorkCompany      varchar(255),
  IN v_WorkStartDate    date,
  IN v_WorkEndDate      date,
  IN v_WorkPosition     varchar(100),
  IN v_WorkDescription  text,
  IN v_HiringCampaign   varchar(100),
  IN v_HiringRound      varchar(50),
  IN v_Rating           varchar(10),
  IN v_JobPosition      varchar(100),
  IN v_IsEmployee       tinyint(1),
  IN v_Department       varchar(100)
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM candidate WHERE CandidateID = v_CandidateID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Ứng viên không tồn tại';
  END IF;

  UPDATE candidate
  SET
    CVFile           = v_CVFile,
    Avatar           = v_Avatar,
    FullName         = v_FullName,
    DateOfBirth      = v_DateOfBirth,
    Gender           = v_Gender,
    City             = v_City,
    PhoneNumber      = v_PhoneNumber,
    Email            = v_Email,
    Country          = v_Country,
    Province         = v_Province,
    Ward             = v_Ward,
    Address          = v_Address,
    Level            = v_Level,
    EducationPlace   = v_EducationPlace,
    Major            = v_Major,
    HiringDate       = v_HiringDate,
    CandidateSource  = v_CandidateSource,
    HRInCharge       = v_HRInCharge,
    Collaborator     = v_Collaborator,
    IsReferenceAdded = v_IsReferenceAdded,
    LastCompany      = v_LastCompany,
    WorkCompany      = v_WorkCompany,
    WorkStartDate    = v_WorkStartDate,
    WorkEndDate      = v_WorkEndDate,
    WorkPosition     = v_WorkPosition,
    WorkDescription  = v_WorkDescription,
    HiringCampaign   = v_HiringCampaign,
    HiringRound      = v_HiringRound,
    Rating           = v_Rating,
    JobPosition      = v_JobPosition,
    IsEmployee       = v_IsEmployee,
    Department       = v_Department
  WHERE CandidateID = v_CandidateID;
END
;;
delimiter ;


-- ----------------------------
-- Procedure: Proc_DeleteCandidateById
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_DeleteCandidateById`;
delimiter ;;
CREATE PROCEDURE `Proc_DeleteCandidateById`(IN v_CandidateID char(36))
BEGIN
  DELETE FROM candidate WHERE CandidateID = v_CandidateID;
END
;;
delimiter ;


-- ----------------------------
-- Procedure: Proc_Candidate_FilterPaging  (dùng cho BaseController /Paging)
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_Candidate_FilterPaging`;
delimiter ;;
CREATE PROCEDURE `Proc_Candidate_FilterPaging`(
  IN v_pageIndex    int,
  IN v_pageSize     int,
  IN v_search       varchar(255),
  IN v_sort         varchar(200),
  IN v_searchFields json
)
BEGIN
  DECLARE v_offset          int;
  DECLARE v_where           text DEFAULT ' WHERE 1=1 ';
  DECLARE v_orderBy         text DEFAULT '';
  DECLARE v_searchCondition text;

  IF v_pageIndex < 1 THEN SET v_pageIndex = 1; END IF;
  IF v_pageSize  < 1 THEN SET v_pageSize  = 20; END IF;
  SET v_offset = (v_pageIndex - 1) * v_pageSize;

  -- Search nhiều field
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

  -- Sort nhiều cột
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
    SET v_orderBy = ' ORDER BY CandidateID DESC';
  END IF;

  SET @v_sql = CONCAT(
    'SELECT * FROM candidate', v_where, v_orderBy,
    ' LIMIT ', v_offset, ',', v_pageSize, ';'
  );

  SET @v_sqlCount = CONCAT('SELECT COUNT(*) AS Total FROM candidate', v_where);

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
-- Procedure: Proc_Candidate_Filter_Paging  (dùng cho /filter endpoint)
-- ----------------------------
DROP PROCEDURE IF EXISTS `Proc_Candidate_Filter_Paging`;
delimiter ;;
CREATE PROCEDURE `Proc_Candidate_Filter_Paging`(
  IN  v_Search          varchar(255),
  IN  v_Gender          varchar(10),
  IN  v_Level           varchar(50),
  IN  v_City            varchar(100),
  IN  v_JobPosition     varchar(100),
  IN  v_Department      varchar(100),
  IN  v_CandidateSource varchar(100),
  IN  v_IsEmployee      tinyint(1),
  IN  v_HiringDateFrom  date,
  IN  v_HiringDateTo    date,
  IN  v_PageSize        int,
  IN  v_PageIndex       int,
  OUT v_Total           bigint
)
BEGIN
  DECLARE v_offset int;
  DECLARE v_where  text DEFAULT ' WHERE 1=1 ';

  IF v_PageIndex < 1 THEN SET v_PageIndex = 1; END IF;
  IF v_PageSize  < 1 THEN SET v_PageSize  = 10; END IF;
  SET v_offset = (v_PageIndex - 1) * v_PageSize;

  -- Search keyword trên FullName, Email, PhoneNumber
  IF v_Search IS NOT NULL AND v_Search <> '' THEN
    SET v_where = CONCAT(v_where,
      ' AND (FullName LIKE ', QUOTE(CONCAT('%', v_Search, '%')),
      ' OR Email LIKE ', QUOTE(CONCAT('%', v_Search, '%')),
      ' OR PhoneNumber LIKE ', QUOTE(CONCAT('%', v_Search, '%')), ')'
    );
  END IF;

  IF v_Gender IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND Gender = ', QUOTE(v_Gender));
  END IF;

  IF v_Level IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND Level = ', QUOTE(v_Level));
  END IF;

  IF v_City IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND City = ', QUOTE(v_City));
  END IF;

  IF v_JobPosition IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND JobPosition = ', QUOTE(v_JobPosition));
  END IF;

  IF v_Department IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND Department = ', QUOTE(v_Department));
  END IF;

  IF v_CandidateSource IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND CandidateSource = ', QUOTE(v_CandidateSource));
  END IF;

  IF v_IsEmployee IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND IsEmployee = ', v_IsEmployee);
  END IF;

  IF v_HiringDateFrom IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND HiringDate >= ', QUOTE(v_HiringDateFrom));
  END IF;

  IF v_HiringDateTo IS NOT NULL THEN
    SET v_where = CONCAT(v_where, ' AND HiringDate <= ', QUOTE(v_HiringDateTo));
  END IF;

  SET @v_sqlCount = CONCAT('SELECT COUNT(*) INTO @_total FROM candidate', v_where);
  PREPARE stmtCount FROM @v_sqlCount;
  EXECUTE stmtCount;
  DEALLOCATE PREPARE stmtCount;
  SET v_Total = @_total;

  SET @v_sqlData = CONCAT(
    'SELECT * FROM candidate',
    v_where,
    ' ORDER BY CandidateID DESC',
    ' LIMIT ', v_PageSize, ' OFFSET ', v_offset
  );
  PREPARE stmtData FROM @v_sqlData;
  EXECUTE stmtData;
  DEALLOCATE PREPARE stmtData;
END
;;
delimiter ;
