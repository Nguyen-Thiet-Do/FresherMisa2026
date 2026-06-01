-- Migration: thêm TaxableFormula và ExemptFormula
-- Áp dụng khi TaxType = PartiallyExempt (3)
-- Run on schema: amis_tien_luong

USE amis_tien_luong;

-- 1. Thêm cột vào bảng thành phần lương đơn vị
ALTER TABLE `pa_salary_composition`
  ADD COLUMN `TaxableFormula` TEXT NULL AFTER `NormFormula`,
  ADD COLUMN `ExemptFormula`  TEXT NULL AFTER `TaxableFormula`;

-- 2. Thêm cột vào bảng thành phần lương hệ thống
ALTER TABLE `pa_salary_composition_system`
  ADD COLUMN `TaxableFormula` TEXT NULL AFTER `NormFormula`,
  ADD COLUMN `ExemptFormula`  TEXT NULL AFTER `TaxableFormula`;

-- 3. Cập nhật stored procedure Insert cho pa_salary_composition
DROP PROCEDURE IF EXISTS Proc_Insertpa_salary_composition;
DELIMITER $$
CREATE PROCEDURE Proc_Insertpa_salary_composition(
  IN v_SalaryCompositionID    CHAR(36),
  IN v_Code                   VARCHAR(255),
  IN v_Name                   VARCHAR(255),
  IN v_OrganizationNames      TEXT,
  IN v_ComponentTypeID        CHAR(36),
  IN v_ComponentTypeName      VARCHAR(255),
  IN v_SystemCompositionID    CHAR(36),
  IN v_Nature                 TINYINT,
  IN v_TaxType                TINYINT,
  IN v_TaxDeductible          TINYINT,
  IN v_ValueType              TINYINT,
  IN v_ValueMode              TINYINT,
  IN v_ValueFormula           TEXT,
  IN v_ValueScope             TINYINT,
  IN v_ValueScopeLevel        TINYINT,
  IN v_SumSourceCompositionID CHAR(36),
  IN v_NormFormula            TEXT,
  IN v_TaxableFormula         TEXT,
  IN v_ExemptFormula          TEXT,
  IN v_AllowExceedNorm        TINYINT,
  IN v_Description            TEXT,
  IN v_ShowOnPayslip          TINYINT,
  IN v_HideWhenZero           TINYINT,
  IN v_Source                 TINYINT,
  IN v_Status                 TINYINT,
  IN v_CreatedBy              VARCHAR(255),
  IN v_CreateDate             DATETIME,
  IN v_ModifiedBy             VARCHAR(255),
  IN v_ModifiedDate           DATETIME,
  IN v_State                  TINYINT,
  IN v_IsDeleted              TINYINT
)
BEGIN
  INSERT INTO `pa_salary_composition` (
    `SalaryCompositionID`, `Code`, `Name`,
    `ComponentTypeID`, `SystemCompositionID`,
    `Nature`, `TaxType`, `TaxDeductible`,
    `ValueType`, `ValueMode`, `ValueFormula`,
    `ValueScope`, `ValueScopeLevel`, `SumSourceCompositionID`,
    `NormFormula`, `TaxableFormula`, `ExemptFormula`,
    `AllowExceedNorm`, `Description`,
    `ShowOnPayslip`, `HideWhenZero`,
    `Source`, `Status`,
    `CreatedBy`, `CreateDate`,
    `ModifiedBy`, `ModifiedDate`,
    `IsDeleted`
  ) VALUES (
    v_SalaryCompositionID, v_Code, v_Name,
    v_ComponentTypeID, v_SystemCompositionID,
    v_Nature, v_TaxType, v_TaxDeductible,
    v_ValueType, v_ValueMode, v_ValueFormula,
    v_ValueScope, v_ValueScopeLevel, v_SumSourceCompositionID,
    v_NormFormula, v_TaxableFormula, v_ExemptFormula,
    v_AllowExceedNorm, v_Description,
    v_ShowOnPayslip, v_HideWhenZero,
    v_Source, v_Status,
    v_CreatedBy, v_CreateDate,
    v_ModifiedBy, v_ModifiedDate,
    0
  );
END$$
DELIMITER ;

-- 4. Cập nhật stored procedure Update cho pa_salary_composition
DROP PROCEDURE IF EXISTS Proc_Updatepa_salary_composition;
DELIMITER $$
CREATE PROCEDURE Proc_Updatepa_salary_composition(
  IN v_SalaryCompositionID    CHAR(36),
  IN v_Code                   VARCHAR(255),
  IN v_Name                   VARCHAR(255),
  IN v_OrganizationNames      TEXT,
  IN v_ComponentTypeID        CHAR(36),
  IN v_ComponentTypeName      VARCHAR(255),
  IN v_SystemCompositionID    CHAR(36),
  IN v_Nature                 TINYINT,
  IN v_TaxType                TINYINT,
  IN v_TaxDeductible          TINYINT,
  IN v_ValueType              TINYINT,
  IN v_ValueMode              TINYINT,
  IN v_ValueFormula           TEXT,
  IN v_ValueScope             TINYINT,
  IN v_ValueScopeLevel        TINYINT,
  IN v_SumSourceCompositionID CHAR(36),
  IN v_NormFormula            TEXT,
  IN v_TaxableFormula         TEXT,
  IN v_ExemptFormula          TEXT,
  IN v_AllowExceedNorm        TINYINT,
  IN v_Description            TEXT,
  IN v_ShowOnPayslip          TINYINT,
  IN v_HideWhenZero           TINYINT,
  IN v_Source                 TINYINT,
  IN v_Status                 TINYINT,
  IN v_CreatedBy              VARCHAR(255),
  IN v_CreateDate             DATETIME,
  IN v_ModifiedBy             VARCHAR(255),
  IN v_ModifiedDate           DATETIME,
  IN v_State                  TINYINT,
  IN v_IsDeleted              TINYINT
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM `pa_salary_composition` WHERE `SalaryCompositionID` = v_SalaryCompositionID AND `IsDeleted` = 0) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Thành phần lương không tồn tại';
  END IF;

  UPDATE `pa_salary_composition` SET
    `Name`                    = v_Name,
    `ComponentTypeID`         = v_ComponentTypeID,
    `SystemCompositionID`     = v_SystemCompositionID,
    `Nature`                  = v_Nature,
    `TaxType`                 = v_TaxType,
    `TaxDeductible`           = v_TaxDeductible,
    `ValueType`               = v_ValueType,
    `ValueMode`               = v_ValueMode,
    `ValueFormula`            = v_ValueFormula,
    `ValueScope`              = v_ValueScope,
    `ValueScopeLevel`         = v_ValueScopeLevel,
    `SumSourceCompositionID`  = v_SumSourceCompositionID,
    `NormFormula`             = v_NormFormula,
    `TaxableFormula`          = v_TaxableFormula,
    `ExemptFormula`           = v_ExemptFormula,
    `AllowExceedNorm`         = v_AllowExceedNorm,
    `Description`             = v_Description,
    `ShowOnPayslip`           = v_ShowOnPayslip,
    `HideWhenZero`            = v_HideWhenZero,
    `Source`                  = v_Source,
    `Status`                  = v_Status,
    `ModifiedBy`              = v_ModifiedBy,
    `ModifiedDate`            = v_ModifiedDate
  WHERE `SalaryCompositionID` = v_SalaryCompositionID AND `IsDeleted` = 0;
END$$
DELIMITER ;

-- 5. Cập nhật stored procedure Insert cho pa_salary_composition_system
DROP PROCEDURE IF EXISTS Proc_Insertpa_salary_composition_system;
DELIMITER $$
CREATE PROCEDURE Proc_Insertpa_salary_composition_system(
  IN v_SystemCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_ComponentTypeID      CHAR(36),
  IN v_ComponentTypeName    VARCHAR(255),
  IN v_OrganizationID       CHAR(36),
  IN v_OrganizationName     VARCHAR(255),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_TaxDeductible        TINYINT,
  IN v_ValueType            TINYINT,
  IN v_ValueFormula         TEXT,
  IN v_DefaultValue         DECIMAL(18,4),
  IN v_NormFormula          TEXT,
  IN v_TaxableFormula       TEXT,
  IN v_ExemptFormula        TEXT,
  IN v_Description          TEXT,
  IN v_ShowOnPayslip        TINYINT,
  IN v_CreatedBy            VARCHAR(255),
  IN v_CreateDate           DATETIME,
  IN v_ModifiedBy           VARCHAR(255),
  IN v_ModifiedDate         DATETIME,
  IN v_State                TINYINT,
  IN v_IsDeleted            TINYINT
)
BEGIN
  INSERT INTO `pa_salary_composition_system` (
    `SystemCompositionID`, `Code`, `Name`,
    `ComponentTypeID`, `OrganizationID`,
    `Nature`, `TaxType`, `TaxDeductible`,
    `ValueType`, `ValueFormula`, `DefaultValue`,
    `NormFormula`, `TaxableFormula`, `ExemptFormula`,
    `Description`, `ShowOnPayslip`,
    `CreatedBy`, `CreateDate`,
    `ModifiedBy`, `ModifiedDate`,
    `IsDeleted`
  ) VALUES (
    v_SystemCompositionID, v_Code, v_Name,
    v_ComponentTypeID, v_OrganizationID,
    v_Nature, v_TaxType, v_TaxDeductible,
    v_ValueType, v_ValueFormula, v_DefaultValue,
    v_NormFormula, v_TaxableFormula, v_ExemptFormula,
    v_Description, v_ShowOnPayslip,
    v_CreatedBy, v_CreateDate,
    v_ModifiedBy, v_ModifiedDate,
    0
  );
END$$
DELIMITER ;

-- 6. Cập nhật stored procedure Update cho pa_salary_composition_system
DROP PROCEDURE IF EXISTS Proc_Updatepa_salary_composition_system;
DELIMITER $$
CREATE PROCEDURE Proc_Updatepa_salary_composition_system(
  IN v_SystemCompositionID  CHAR(36),
  IN v_Code                 VARCHAR(255),
  IN v_Name                 VARCHAR(255),
  IN v_ComponentTypeID      CHAR(36),
  IN v_ComponentTypeName    VARCHAR(255),
  IN v_OrganizationID       CHAR(36),
  IN v_OrganizationName     VARCHAR(255),
  IN v_Nature               TINYINT,
  IN v_TaxType              TINYINT,
  IN v_TaxDeductible        TINYINT,
  IN v_ValueType            TINYINT,
  IN v_ValueFormula         TEXT,
  IN v_DefaultValue         DECIMAL(18,4),
  IN v_NormFormula          TEXT,
  IN v_TaxableFormula       TEXT,
  IN v_ExemptFormula        TEXT,
  IN v_Description          TEXT,
  IN v_ShowOnPayslip        TINYINT,
  IN v_CreatedBy            VARCHAR(255),
  IN v_CreateDate           DATETIME,
  IN v_ModifiedBy           VARCHAR(255),
  IN v_ModifiedDate         DATETIME,
  IN v_State                TINYINT,
  IN v_IsDeleted            TINYINT
)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM `pa_salary_composition_system` WHERE `SystemCompositionID` = v_SystemCompositionID) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Thành phần lương hệ thống không tồn tại';
  END IF;

  UPDATE `pa_salary_composition_system` SET
    `Name`            = v_Name,
    `ComponentTypeID` = v_ComponentTypeID,
    `OrganizationID`  = v_OrganizationID,
    `Nature`          = v_Nature,
    `TaxType`         = v_TaxType,
    `TaxDeductible`   = v_TaxDeductible,
    `ValueType`       = v_ValueType,
    `ValueFormula`    = v_ValueFormula,
    `DefaultValue`    = v_DefaultValue,
    `NormFormula`     = v_NormFormula,
    `TaxableFormula`  = v_TaxableFormula,
    `ExemptFormula`   = v_ExemptFormula,
    `Description`     = v_Description,
    `ShowOnPayslip`   = v_ShowOnPayslip,
    `ModifiedBy`      = v_ModifiedBy,
    `ModifiedDate`    = v_ModifiedDate
  WHERE `SystemCompositionID` = v_SystemCompositionID;
END$$
DELIMITER ;
