-- Fix SQLi defense-in-depth: thêm REGEXP validation cho field names trong 2 SP filter paging
-- v_search đã dùng QUOTE() nên an toàn, chỉ cần validate field names (sort, filter, columns)
-- C# đã validate ở tầng service nhưng SP nên tự bảo vệ độc lập

USE amis_tien_luong;

-- ============================================================
-- 1. proc_pa_salary_composition_system_advanced_filter_paging
-- ============================================================
DROP PROCEDURE IF EXISTS proc_pa_salary_composition_system_advanced_filter_paging;

DELIMITER $$
CREATE PROCEDURE proc_pa_salary_composition_system_advanced_filter_paging(
  IN v_page_index        INT,
  IN v_page_size         INT,
  IN v_sort              VARCHAR(500),
  IN v_search            VARCHAR(255),
  IN v_search_fields     JSON,
  IN v_component_type_id CHAR(36),
  IN v_filters           JSON,
  IN v_filter_logic      TINYINT,
  IN v_columns           JSON,
  IN v_exclude_inherited TINYINT
)
BEGIN
  DECLARE v_offset       INT;
  DECLARE v_i            INT          DEFAULT 0;
  DECLARE v_filter_count INT          DEFAULT 0;
  DECLARE v_field        VARCHAR(255);
  DECLARE v_op           VARCHAR(50);
  DECLARE v_value        TEXT;
  DECLARE v_value_to     TEXT;
  DECLARE v_cond         TEXT;
  DECLARE v_j            INT  DEFAULT 0;
  DECLARE v_in_count     INT  DEFAULT 0;
  DECLARE v_in_list      TEXT DEFAULT '';
  DECLARE v_where        TEXT DEFAULT '1=1';
  DECLARE v_user_conds   TEXT DEFAULT NULL;
  DECLARE v_search_or    TEXT DEFAULT NULL;
  DECLARE v_order_by     TEXT DEFAULT NULL;
  DECLARE v_sort_count   INT  DEFAULT 0;
  DECLARE v_sort_i       INT  DEFAULT 1;
  DECLARE v_sort_part    VARCHAR(255);
  DECLARE v_sort_field   VARCHAR(255);
  DECLARE v_sort_dir     VARCHAR(10);
  DECLARE v_select_cols  TEXT DEFAULT 'sc.*, ct.`name` AS component_type_name';
  DECLARE v_col_count    INT  DEFAULT 0;
  DECLARE v_col_i        INT  DEFAULT 0;
  DECLARE v_col_name     VARCHAR(255);

  IF v_page_index < 1 THEN SET v_page_index = 1; END IF;
  IF v_page_size  < 1 THEN SET v_page_size  = 20; END IF;
  SET v_offset = (v_page_index - 1) * v_page_size;

  IF v_exclude_inherited = 1 THEN
    SET v_where = CONCAT(v_where, ' AND sc.`system_composition_id` NOT IN (SELECT `system_composition_id` FROM pa_salary_composition WHERE `source` = 2 AND `is_deleted` = 0)');
  END IF;

  -- Search (v_search đã được bảo vệ bằng QUOTE())
  IF v_search IS NOT NULL AND v_search <> '' AND v_search_fields IS NOT NULL AND JSON_LENGTH(v_search_fields) > 0 THEN
    SELECT GROUP_CONCAT(
      CONCAT('sc.`', JSON_UNQUOTE(JSON_EXTRACT(v_search_fields, CONCAT('$[', n, ']'))), '` LIKE ', QUOTE(CONCAT('%', v_search, '%')))
      SEPARATOR ' OR '
    ) INTO v_search_or
    FROM (SELECT 0 n UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4
          UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9) t
    WHERE n < JSON_LENGTH(v_search_fields);
    IF v_search_or IS NOT NULL THEN SET v_where = CONCAT(v_where, ' AND (', v_search_or, ')'); END IF;
  END IF;

  IF v_component_type_id IS NOT NULL AND v_component_type_id <> '' THEN
    SET v_where = CONCAT(v_where, ' AND sc.`component_type_id` = ', QUOTE(v_component_type_id));
  END IF;

  -- Filters
  IF v_filters IS NOT NULL AND JSON_LENGTH(v_filters) > 0 THEN
    SET v_filter_count = JSON_LENGTH(v_filters);
    WHILE v_i < v_filter_count DO
      SET v_field    = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].field')));
      SET v_op       = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].operator')));
      SET v_cond     = NULL;
      SET v_value    = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))));
      SET v_value_to = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))));

      -- Validate field name trước khi nối vào SQL
      IF v_field NOT REGEXP '^[A-Za-z0-9_]+$' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tên cột filter không hợp lệ';
      END IF;

      IF      v_op = 'Eq'          THEN SET v_cond = CONCAT('sc.`', v_field, '` = ',  QUOTE(v_value));
      ELSEIF  v_op = 'Neq'         THEN SET v_cond = CONCAT('sc.`', v_field, '` <> ', QUOTE(v_value));
      ELSEIF  v_op = 'Contains'    THEN SET v_cond = CONCAT('sc.`', v_field, '` LIKE CONCAT("%", ', QUOTE(v_value), ', "%")');
      ELSEIF  v_op = 'NotContains' THEN SET v_cond = CONCAT('sc.`', v_field, '` NOT LIKE CONCAT("%", ', QUOTE(v_value), ', "%")');
      ELSEIF  v_op = 'StartsWith'  THEN SET v_cond = CONCAT('sc.`', v_field, '` LIKE CONCAT(', QUOTE(v_value), ', "%")');
      ELSEIF  v_op = 'EndsWith'    THEN SET v_cond = CONCAT('sc.`', v_field, '` LIKE CONCAT("%", ', QUOTE(v_value), ')');
      ELSEIF  v_op = 'Empty'       THEN SET v_cond = CONCAT('(sc.`', v_field, '` IS NULL OR sc.`', v_field, '` = "")');
      ELSEIF  v_op = 'NotEmpty'    THEN SET v_cond = CONCAT('(sc.`', v_field, '` IS NOT NULL AND sc.`', v_field, '` <> "")');
      ELSEIF  v_op = 'Gt'          THEN SET v_cond = CONCAT('sc.`', v_field, '` > ',  QUOTE(v_value));
      ELSEIF  v_op = 'Lt'          THEN SET v_cond = CONCAT('sc.`', v_field, '` < ',  QUOTE(v_value));
      ELSEIF  v_op = 'Gte'         THEN SET v_cond = CONCAT('sc.`', v_field, '` >= ', QUOTE(v_value));
      ELSEIF  v_op = 'Lte'         THEN SET v_cond = CONCAT('sc.`', v_field, '` <= ', QUOTE(v_value));
      ELSEIF  v_op = 'Between'     THEN SET v_cond = CONCAT('sc.`', v_field, '` BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
      ELSEIF  v_op = 'NotBetween'  THEN SET v_cond = CONCAT('sc.`', v_field, '` NOT BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
      ELSEIF  v_op = 'Today'       THEN SET v_cond = CONCAT('DATE(sc.`', v_field, '`) = CURDATE()');
      ELSEIF  v_op = 'ThisMonth'   THEN SET v_cond = CONCAT('sc.`', v_field, '` BETWEEN DATE_FORMAT(CURDATE(), "%Y-%m-01") AND LAST_DAY(CURDATE())');
      ELSEIF  v_op = 'ThisYear'    THEN SET v_cond = CONCAT('sc.`', v_field, '` BETWEEN DATE_FORMAT(CURDATE(), "%Y-01-01") AND DATE_FORMAT(CURDATE(), "%Y-12-31")');
      ELSEIF  v_op = 'LastNDays'   THEN SET v_cond = CONCAT('sc.`', v_field, '` >= DATE_SUB(CURDATE(), INTERVAL ', CAST(CAST(v_value AS UNSIGNED) AS CHAR), ' DAY)');
      ELSEIF v_op = 'In' OR v_op = 'NotIn' THEN
        SET v_in_list = ''; SET v_j = 0; SET v_in_count = 0;
        IF JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values')) IS NOT NULL THEN
          SET v_in_count = JSON_LENGTH(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values')));
          WHILE v_j < v_in_count DO
            IF v_j > 0 THEN SET v_in_list = CONCAT(v_in_list, ', '); END IF;
            SET v_in_list = CONCAT(v_in_list, QUOTE(JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values[', v_j, ']')))));
            SET v_j = v_j + 1;
          END WHILE;
        END IF;
        SET v_cond = IF(v_in_list <> '',
          CONCAT('sc.`', v_field, IF(v_op = 'In', '` IN (', '` NOT IN ('), v_in_list, ')'),
          IF(v_op = 'In', '1 = 0', '1 = 1'));
      END IF;

      IF v_cond IS NOT NULL THEN
        IF v_user_conds IS NULL THEN SET v_user_conds = v_cond;
        ELSE SET v_user_conds = CONCAT(v_user_conds, IF(IFNULL(v_filter_logic, 0) = 1, ' OR ', ' AND '), v_cond);
        END IF;
      END IF;
      SET v_i = v_i + 1;
    END WHILE;

    IF v_user_conds IS NOT NULL THEN
      SET v_where = CONCAT(v_where, ' AND (', v_user_conds, ')');
    END IF;
  END IF;

  -- Sort
  IF v_sort IS NOT NULL AND v_sort <> '' THEN
    SET v_sort_count = 1 + (LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', '')));
    SET v_sort_i = 1; SET v_order_by = 'ORDER BY ';
    WHILE v_sort_i <= v_sort_count DO
      SET v_sort_part = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(v_sort, ',', v_sort_i), ',', -1));
      IF    LEFT(v_sort_part, 1) = '-' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'DESC';
      ELSEIF LEFT(v_sort_part, 1) = '+' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'ASC';
      ELSE SET v_sort_field = v_sort_part; SET v_sort_dir = 'ASC';
      END IF;

      -- Validate sort field name trước khi nối vào SQL
      IF v_sort_field NOT REGEXP '^[A-Za-z0-9_]+$' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tên cột sắp xếp không hợp lệ';
      END IF;

      IF v_sort_i > 1 THEN SET v_order_by = CONCAT(v_order_by, ', '); END IF;
      SET v_order_by = CONCAT(v_order_by, 'sc.`', v_sort_field, '` ', v_sort_dir);
      SET v_sort_i = v_sort_i + 1;
    END WHILE;
  ELSE
    SET v_order_by = 'ORDER BY sc.`create_date` DESC';
  END IF;

  -- Columns
  IF v_columns IS NOT NULL AND JSON_LENGTH(v_columns) > 0 THEN
    SET v_col_count = JSON_LENGTH(v_columns);
    SET v_select_cols = '';
    WHILE v_col_i < v_col_count DO
      SET v_col_name = JSON_UNQUOTE(JSON_EXTRACT(v_columns, CONCAT('$[', v_col_i, ']')));

      -- Validate column name trước khi nối vào SQL
      IF v_col_name NOT REGEXP '^[A-Za-z0-9_]+$' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tên cột select không hợp lệ';
      END IF;

      IF v_col_i > 0 THEN SET v_select_cols = CONCAT(v_select_cols, ', '); END IF;
      IF v_col_name = 'component_type_name' THEN
        SET v_select_cols = CONCAT(v_select_cols, 'ct.`name` AS component_type_name');
      ELSE
        SET v_select_cols = CONCAT(v_select_cols, 'sc.`', v_col_name, '`');
      END IF;
      SET v_col_i = v_col_i + 1;
    END WHILE;
  END IF;

  SET @v_sql = CONCAT(
    'SELECT ', v_select_cols,
    ' FROM pa_salary_composition_system sc ',
    'LEFT JOIN pa_salary_component_type ct ON sc.`component_type_id` = ct.`component_type_id` WHERE ',
    v_where, ' ', v_order_by, ' LIMIT ', v_offset, ',', v_page_size);
  SET @v_count_sql = CONCAT('SELECT COUNT(*) AS total FROM pa_salary_composition_system sc WHERE ', v_where);
  PREPARE stmt FROM @v_sql;       EXECUTE stmt; DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @v_count_sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END$$

-- ============================================================
-- 2. proc_pa_salary_composition_advanced_filter_paging
-- ============================================================
DROP PROCEDURE IF EXISTS proc_pa_salary_composition_advanced_filter_paging;

CREATE PROCEDURE proc_pa_salary_composition_advanced_filter_paging(
  IN v_page_index       INT,
  IN v_page_size        INT,
  IN v_sort             VARCHAR(500),
  IN v_search           VARCHAR(255),
  IN v_search_fields    JSON,
  IN v_status           TINYINT,
  IN v_org_ids          JSON,
  IN v_filters          JSON,
  IN v_filter_logic     TINYINT
)
BEGIN
  DECLARE v_offset       INT;
  DECLARE v_i            INT          DEFAULT 0;
  DECLARE v_filter_count INT          DEFAULT 0;
  DECLARE v_field        VARCHAR(255);
  DECLARE v_op           VARCHAR(50);
  DECLARE v_value        TEXT;
  DECLARE v_value_to     TEXT;
  DECLARE v_cond         TEXT;
  DECLARE v_j        INT  DEFAULT 0;
  DECLARE v_in_count INT  DEFAULT 0;
  DECLARE v_in_list  TEXT DEFAULT '';
  DECLARE v_where       TEXT DEFAULT 'sc.`is_deleted` = 0';
  DECLARE v_user_conds  TEXT DEFAULT NULL;
  DECLARE v_search_or   TEXT DEFAULT NULL;
  DECLARE v_order_by    TEXT DEFAULT NULL;
  DECLARE v_sort_count  INT  DEFAULT 0;
  DECLARE v_sort_i      INT  DEFAULT 1;
  DECLARE v_sort_part   VARCHAR(255);
  DECLARE v_sort_field  VARCHAR(255);
  DECLARE v_sort_dir    VARCHAR(10);

  IF v_page_index < 1 THEN SET v_page_index = 1; END IF;
  IF v_page_size  < 1 THEN SET v_page_size  = 20; END IF;
  SET v_offset = (v_page_index - 1) * v_page_size;

  -- Search (v_search đã được bảo vệ bằng QUOTE())
  IF v_search IS NOT NULL AND v_search <> '' AND v_search_fields IS NOT NULL AND JSON_LENGTH(v_search_fields) > 0 THEN
    SELECT GROUP_CONCAT(
      CONCAT('sc.`', JSON_UNQUOTE(JSON_EXTRACT(v_search_fields, CONCAT('$[', n, ']'))), '` LIKE ', QUOTE(CONCAT('%', v_search, '%')))
      SEPARATOR ' OR '
    ) INTO v_search_or
    FROM (SELECT 0 n UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4
          UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9) t
    WHERE n < JSON_LENGTH(v_search_fields);
    IF v_search_or IS NOT NULL THEN SET v_where = CONCAT(v_where, ' AND (', v_search_or, ')'); END IF;
  END IF;

  IF v_status IS NOT NULL THEN SET v_where = CONCAT(v_where, ' AND sc.`status` = ', v_status); END IF;

  IF v_org_ids IS NOT NULL AND JSON_LENGTH(v_org_ids) > 0 THEN
    BEGIN
      DECLARE v_org_i   INT DEFAULT 0;
      DECLARE v_org_len INT DEFAULT JSON_LENGTH(v_org_ids);
      DECLARE v_in_orgs TEXT DEFAULT '';
      WHILE v_org_i < v_org_len DO
        IF v_org_i > 0 THEN SET v_in_orgs = CONCAT(v_in_orgs, ', '); END IF;
        SET v_in_orgs = CONCAT(v_in_orgs, QUOTE(JSON_UNQUOTE(JSON_EXTRACT(v_org_ids, CONCAT('$[', v_org_i, ']')))));
        SET v_org_i = v_org_i + 1;
      END WHILE;
      SET v_where = CONCAT(v_where, ' AND sc.`salary_composition_id` IN (SELECT `salary_composition_id` FROM pa_salary_composition_organization WHERE `organization_id` IN (', v_in_orgs, '))');
    END;
  END IF;

  -- Filters
  IF v_filters IS NOT NULL AND JSON_LENGTH(v_filters) > 0 THEN
    SET v_filter_count = JSON_LENGTH(v_filters);
    WHILE v_i < v_filter_count DO
      SET v_field = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].field')));
      SET v_op    = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].operator')));
      SET v_cond  = NULL;
      SET v_value = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))));
      SET v_value_to = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))));

      -- Validate field name trước khi nối vào SQL
      IF v_field NOT REGEXP '^[A-Za-z0-9_]+$' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tên cột filter không hợp lệ';
      END IF;

      IF      v_op = 'Eq'          THEN SET v_cond = CONCAT('sc.`', v_field, '` = ',  QUOTE(v_value));
      ELSEIF  v_op = 'Neq'         THEN SET v_cond = CONCAT('sc.`', v_field, '` <> ', QUOTE(v_value));
      ELSEIF  v_op = 'Contains'    THEN SET v_cond = CONCAT('sc.`', v_field, '` LIKE CONCAT("%", ', QUOTE(v_value), ', "%")');
      ELSEIF  v_op = 'NotContains' THEN SET v_cond = CONCAT('sc.`', v_field, '` NOT LIKE CONCAT("%", ', QUOTE(v_value), ', "%")');
      ELSEIF  v_op = 'StartsWith'  THEN SET v_cond = CONCAT('sc.`', v_field, '` LIKE CONCAT(', QUOTE(v_value), ', "%")');
      ELSEIF  v_op = 'EndsWith'    THEN SET v_cond = CONCAT('sc.`', v_field, '` LIKE CONCAT("%", ', QUOTE(v_value), ')');
      ELSEIF  v_op = 'Empty'       THEN SET v_cond = CONCAT('(sc.`', v_field, '` IS NULL OR sc.`', v_field, '` = "")');
      ELSEIF  v_op = 'NotEmpty'    THEN SET v_cond = CONCAT('(sc.`', v_field, '` IS NOT NULL AND sc.`', v_field, '` <> "")');
      ELSEIF  v_op = 'Gt'          THEN SET v_cond = CONCAT('sc.`', v_field, '` > ',  QUOTE(v_value));
      ELSEIF  v_op = 'Lt'          THEN SET v_cond = CONCAT('sc.`', v_field, '` < ',  QUOTE(v_value));
      ELSEIF  v_op = 'Gte'         THEN SET v_cond = CONCAT('sc.`', v_field, '` >= ', QUOTE(v_value));
      ELSEIF  v_op = 'Lte'         THEN SET v_cond = CONCAT('sc.`', v_field, '` <= ', QUOTE(v_value));
      ELSEIF  v_op = 'Between'     THEN SET v_cond = CONCAT('sc.`', v_field, '` BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
      ELSEIF  v_op = 'NotBetween'  THEN SET v_cond = CONCAT('sc.`', v_field, '` NOT BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
      ELSEIF  v_op = 'Today'       THEN SET v_cond = CONCAT('DATE(sc.`', v_field, '`) = CURDATE()');
      ELSEIF  v_op = 'ThisMonth'   THEN SET v_cond = CONCAT('sc.`', v_field, '` BETWEEN DATE_FORMAT(CURDATE(), "%Y-%m-01") AND LAST_DAY(CURDATE())');
      ELSEIF  v_op = 'ThisYear'    THEN SET v_cond = CONCAT('sc.`', v_field, '` BETWEEN DATE_FORMAT(CURDATE(), "%Y-01-01") AND DATE_FORMAT(CURDATE(), "%Y-12-31")');
      ELSEIF  v_op = 'LastNDays'   THEN SET v_cond = CONCAT('sc.`', v_field, '` >= DATE_SUB(CURDATE(), INTERVAL ', CAST(CAST(v_value AS UNSIGNED) AS CHAR), ' DAY)');
      ELSEIF v_op = 'In' OR v_op = 'NotIn' THEN
        SET v_in_list = ''; SET v_j = 0; SET v_in_count = 0;
        IF JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values')) IS NOT NULL THEN
          SET v_in_count = JSON_LENGTH(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values')));
          WHILE v_j < v_in_count DO
            IF v_j > 0 THEN SET v_in_list = CONCAT(v_in_list, ', '); END IF;
            SET v_in_list = CONCAT(v_in_list, QUOTE(JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].values[', v_j, ']')))));
            SET v_j = v_j + 1;
          END WHILE;
        END IF;
        SET v_cond = IF(v_in_list <> '',
          CONCAT('sc.`', v_field, IF(v_op = 'In', '` IN (', '` NOT IN ('), v_in_list, ')'),
          IF(v_op = 'In', '1 = 0', '1 = 1'));
      END IF;

      IF v_cond IS NOT NULL THEN
        IF v_user_conds IS NULL THEN SET v_user_conds = v_cond;
        ELSE SET v_user_conds = CONCAT(v_user_conds, IF(IFNULL(v_filter_logic, 0) = 1, ' OR ', ' AND '), v_cond);
        END IF;
      END IF;
      SET v_i = v_i + 1;
    END WHILE;

    IF v_user_conds IS NOT NULL THEN
      SET v_where = CONCAT(v_where, ' AND (', v_user_conds, ')');
    END IF;
  END IF;

  -- Sort
  IF v_sort IS NOT NULL AND v_sort <> '' THEN
    SET v_sort_count = 1 + (LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', '')));
    SET v_sort_i = 1; SET v_order_by = 'ORDER BY ';
    WHILE v_sort_i <= v_sort_count DO
      SET v_sort_part = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(v_sort, ',', v_sort_i), ',', -1));
      IF    LEFT(v_sort_part, 1) = '-' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'DESC';
      ELSEIF LEFT(v_sort_part, 1) = '+' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'ASC';
      ELSE SET v_sort_field = v_sort_part; SET v_sort_dir = 'ASC';
      END IF;

      -- Validate sort field name trước khi nối vào SQL
      IF v_sort_field NOT REGEXP '^[A-Za-z0-9_]+$' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tên cột sắp xếp không hợp lệ';
      END IF;

      IF v_sort_i > 1 THEN SET v_order_by = CONCAT(v_order_by, ', '); END IF;
      SET v_order_by = CONCAT(v_order_by, 'sc.`', v_sort_field, '` ', v_sort_dir);
      SET v_sort_i = v_sort_i + 1;
    END WHILE;
  ELSE
    SET v_order_by = 'ORDER BY sc.`create_date` DESC';
  END IF;

  SET @v_sql = CONCAT(
    'SELECT sc.*, ct.`name` AS component_type_name, ',
    '(SELECT GROUP_CONCAT(sco.`organization_id` SEPARATOR '','') FROM pa_salary_composition_organization sco WHERE sco.`salary_composition_id` = sc.`salary_composition_id`) AS organization_ids, ',
    '(SELECT GROUP_CONCAT(org.`name` SEPARATOR '', '') FROM pa_salary_composition_organization sco2 JOIN pa_organization org ON sco2.`organization_id` = org.`organization_id` WHERE sco2.`salary_composition_id` = sc.`salary_composition_id`) AS organization_names ',
    'FROM pa_salary_composition sc ',
    'LEFT JOIN pa_salary_component_type ct ON sc.`component_type_id` = ct.`component_type_id` WHERE ',
    v_where, ' ', v_order_by, ' LIMIT ', v_offset, ',', v_page_size);
  SET @v_count_sql = CONCAT('SELECT COUNT(*) AS total FROM pa_salary_composition sc WHERE ', v_where);
  PREPARE stmt FROM @v_sql;       EXECUTE stmt; DEALLOCATE PREPARE stmt;
  PREPARE stmt FROM @v_count_sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END$$

DELIMITER ;
