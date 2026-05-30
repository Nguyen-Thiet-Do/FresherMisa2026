USE amis_tien_luong;

-- 1. Xóa 2 cột khỏi bảng
ALTER TABLE pa_salary_composition_system
    DROP COLUMN Source,
    DROP COLUMN Status;

-- 2. Cập nhật Proc Insert — bỏ v_Source, v_Status
DROP PROCEDURE IF EXISTS Proc_Insertpa_salary_composition_system;
DELIMITER ;;
CREATE PROCEDURE Proc_Insertpa_salary_composition_system(
  IN v_SystemCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_ComponentTypeID      CHAR(36),
  IN v_OrganizationID       CHAR(36),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_TaxDeductible        TINYINT(1),
  IN v_ValueType            TINYINT,
  IN v_ValueFormula         TEXT,
  IN v_NormFormula          TEXT,
  IN v_Description          VARCHAR(1000),
  IN v_ShowOnPayslip        TINYINT(1),
  IN v_CreatedBy            VARCHAR(100),
  IN v_CreateDate           DATETIME
)
BEGIN
  IF v_TaxType IS NOT NULL AND v_Nature <> 1 THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập';
  END IF;

  INSERT INTO pa_salary_composition_system (
    SystemCompositionID, Code, Name, ComponentTypeID, OrganizationID,
    Nature, TaxType, TaxDeductible, ValueType, ValueFormula,
    NormFormula, Description, ShowOnPayslip,
    CreatedBy, CreateDate
  ) VALUES (
    v_SystemCompositionID, v_Code, v_Name, v_ComponentTypeID, v_OrganizationID,
    v_Nature, v_TaxType, IFNULL(v_TaxDeductible, 0), IFNULL(v_ValueType, 1), v_ValueFormula,
    v_NormFormula, v_Description, IFNULL(v_ShowOnPayslip, 1),
    v_CreatedBy, v_CreateDate
  );
END
;;
DELIMITER ;

-- 3. Cập nhật Proc Update — bỏ v_Source, v_Status
DROP PROCEDURE IF EXISTS Proc_Updatepa_salary_composition_system;
DELIMITER ;;
CREATE PROCEDURE Proc_Updatepa_salary_composition_system(
  IN v_SystemCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_ComponentTypeID      CHAR(36),
  IN v_OrganizationID       CHAR(36),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_TaxDeductible        TINYINT(1),
  IN v_ValueType            TINYINT,
  IN v_ValueFormula         TEXT,
  IN v_NormFormula          TEXT,
  IN v_Description          VARCHAR(1000),
  IN v_ShowOnPayslip        TINYINT(1),
  IN v_ModifiedBy           VARCHAR(100),
  IN v_ModifiedDate         DATETIME
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pa_salary_composition_system WHERE SystemCompositionID = v_SystemCompositionID) THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Thành phần lương hệ thống không tồn tại';
  END IF;

  IF v_TaxType IS NOT NULL AND v_Nature <> 1 THEN
    SIGNAL SQLSTATE '45000'
    SET MESSAGE_TEXT = 'Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập';
  END IF;

  UPDATE pa_salary_composition_system
  SET Code            = v_Code,
      Name            = v_Name,
      ComponentTypeID = v_ComponentTypeID,
      OrganizationID  = v_OrganizationID,
      Nature          = v_Nature,
      TaxType         = v_TaxType,
      TaxDeductible   = IFNULL(v_TaxDeductible, 0),
      ValueType       = v_ValueType,
      ValueFormula    = v_ValueFormula,
      NormFormula     = v_NormFormula,
      Description     = v_Description,
      ShowOnPayslip   = IFNULL(v_ShowOnPayslip, 1),
      ModifiedBy      = v_ModifiedBy,
      ModifiedDate    = v_ModifiedDate
  WHERE SystemCompositionID = v_SystemCompositionID;
END
;;
DELIMITER ;

-- 4. Cập nhật Proc Filter — bỏ v_Status, v_Source và logic lọc tương ứng
DROP PROCEDURE IF EXISTS Proc_pa_salary_composition_system_Filter;
DELIMITER ;;
CREATE PROCEDURE Proc_pa_salary_composition_system_Filter(
  IN v_Search           VARCHAR(255),
  IN v_OrganizationID   CHAR(36),
  IN v_ComponentTypeID  CHAR(36),
  IN v_Nature           TINYINT,
  IN v_PageIndex        INT,
  IN v_PageSize         INT
)
BEGIN
  DECLARE v_offset INT;

  IF v_PageIndex < 1 THEN SET v_PageIndex = 1; END IF;
  IF v_PageSize  < 1 THEN SET v_PageSize  = 20; END IF;
  SET v_offset = (v_PageIndex - 1) * v_PageSize;

  SET @cond = ' AND SystemCompositionID NOT IN (SELECT SystemCompositionID FROM pa_salary_composition WHERE Source = 2 AND IsDeleted = 0 AND SystemCompositionID IS NOT NULL)';

  IF v_Search IS NOT NULL AND v_Search <> '' THEN
    SET @cond = CONCAT(@cond, ' AND (sc.Code LIKE "%', v_Search, '%" OR sc.Name LIKE "%', v_Search, '%")');
  END IF;
  IF v_OrganizationID IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND sc.OrganizationID = "', v_OrganizationID, '"');
  END IF;
  IF v_ComponentTypeID IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND sc.ComponentTypeID = "', v_ComponentTypeID, '"');
  END IF;
  IF v_Nature IS NOT NULL THEN
    SET @cond = CONCAT(@cond, ' AND sc.Nature = ', v_Nature);
  END IF;

  SET @sql = CONCAT(
    'SELECT sc.*, sct.Name AS ComponentTypeName, org.Name AS OrganizationName ',
    'FROM pa_salary_composition_system sc ',
    'LEFT JOIN pa_salary_component_type sct ON sc.ComponentTypeID = sct.ComponentTypeID ',
    'LEFT JOIN pa_organization org ON sc.OrganizationID = org.OrganizationID ',
    'WHERE 1=1', @cond,
    ' ORDER BY sc.CreateDate DESC LIMIT ', v_offset, ',', v_PageSize
  );
  SET @sqlCount = CONCAT(
    'SELECT COUNT(*) AS Total FROM pa_salary_composition_system sc WHERE 1=1', @cond
  );

  PREPARE stmt FROM @sql;      EXECUTE stmt;      DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @sqlCount; EXECUTE stmt;      DEALLOCATE PREPARE stmt;
END
;;
DELIMITER ;
