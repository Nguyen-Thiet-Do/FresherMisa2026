-- ============================================================
-- ADVANCED FILTER STORED PROCEDURES
-- Approach 2: SP nhận filters dạng JSON từ C#, tự build WHERE
-- Endpoint: POST /api/{entity}/AdvancedFilterProc
--
-- So sánh với Approach 1 (Dynamic SQL trong C#):
--   Approach 1 → C# build SQL, Dapper parameterize values   → an toàn, không cần SP
--   Approach 2 → C# validate fields + serialize JSON → SP   → logic ở MySQL, cần tạo SP từng entity
-- ============================================================

USE misaemployee_development;

DELIMITER $$

-- ============================================================
-- Employee
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Employee_AdvancedFilterPaging`$$
CREATE PROCEDURE `Proc_Employee_AdvancedFilterPaging`(
    IN v_pageIndex INT,
    IN v_pageSize  INT,
    IN v_sort      VARCHAR(500),
    IN v_filters   JSON
)
BEGIN
    DECLARE v_offset INT;

    DECLARE v_i            INT         DEFAULT 0;
    DECLARE v_filter_count INT         DEFAULT 0;
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
        SET v_order_by = 'ORDER BY `EmployeeID` DESC';
    END IF;

    -- Execute
    SET v_sql       = CONCAT('SELECT * FROM `Employee`',       IF(v_where != '', CONCAT(' WHERE ', v_where), ''), ' ', v_order_by, ' LIMIT ', v_pageSize, ' OFFSET ', v_offset);
    SET v_count_sql = CONCAT('SELECT COUNT(*) FROM `Employee`', IF(v_where != '', CONCAT(' WHERE ', v_where), ''));

    PREPARE stmt FROM v_sql;       EXECUTE stmt;       DEALLOCATE PREPARE stmt;
    PREPARE stmt FROM v_count_sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END$$


-- ============================================================
-- Department
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Department_AdvancedFilterPaging`$$
CREATE PROCEDURE `Proc_Department_AdvancedFilterPaging`(
    IN v_pageIndex INT,
    IN v_pageSize  INT,
    IN v_sort      VARCHAR(500),
    IN v_filters   JSON
)
BEGIN
    DECLARE v_offset INT;
    DECLARE v_i INT DEFAULT 0; DECLARE v_filter_count INT DEFAULT 0;
    DECLARE v_field VARCHAR(255) DEFAULT ''; DECLARE v_op VARCHAR(50) DEFAULT '';
    DECLARE v_value TEXT DEFAULT NULL; DECLARE v_value_to TEXT DEFAULT NULL;
    DECLARE v_cond TEXT DEFAULT NULL; DECLARE v_where TEXT DEFAULT '';
    DECLARE v_j INT DEFAULT 0; DECLARE v_in_count INT DEFAULT 0; DECLARE v_in_list TEXT DEFAULT '';
    DECLARE v_sort_count INT DEFAULT 0; DECLARE v_sort_i INT DEFAULT 1;
    DECLARE v_sort_part VARCHAR(255) DEFAULT ''; DECLARE v_sort_field VARCHAR(255) DEFAULT '';
    DECLARE v_sort_dir VARCHAR(10) DEFAULT 'ASC'; DECLARE v_order_by TEXT DEFAULT '';
    DECLARE v_sql TEXT; DECLARE v_count_sql TEXT;

    IF v_pageIndex IS NULL OR v_pageIndex < 1 THEN SET v_pageIndex = 1; END IF;
    IF v_pageSize  IS NULL OR v_pageSize  < 1 THEN SET v_pageSize  = 20; END IF;
    SET v_offset = (v_pageIndex - 1) * v_pageSize;

    IF v_filters IS NOT NULL AND JSON_LENGTH(v_filters) > 0 THEN
        SET v_filter_count = JSON_LENGTH(v_filters);
        WHILE v_i < v_filter_count DO
            SET v_field = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].field')));
            SET v_op    = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].operator')));
            SET v_cond  = NULL;
            SET v_value = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))));
            SET v_value_to = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))));

            IF      v_op = 'Eq'          THEN SET v_cond = CONCAT('`', v_field, '` = ', QUOTE(v_value));
            ELSEIF  v_op = 'Neq'         THEN SET v_cond = CONCAT('`', v_field, '` <> ', QUOTE(v_value));
            ELSEIF  v_op = 'Contains'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'NotContains' THEN SET v_cond = CONCAT('`', v_field, '` NOT LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'StartsWith'  THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'EndsWith'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ')');
            ELSEIF  v_op = 'Empty'       THEN SET v_cond = CONCAT('(`', v_field, '` IS NULL OR `', v_field, '` = '''')');
            ELSEIF  v_op = 'NotEmpty'    THEN SET v_cond = CONCAT('(`', v_field, '` IS NOT NULL AND `', v_field, '` <> '''')');
            ELSEIF  v_op = 'Gt'          THEN SET v_cond = CONCAT('`', v_field, '` > ', QUOTE(v_value));
            ELSEIF  v_op = 'Lt'          THEN SET v_cond = CONCAT('`', v_field, '` < ', QUOTE(v_value));
            ELSEIF  v_op = 'Gte'         THEN SET v_cond = CONCAT('`', v_field, '` >= ', QUOTE(v_value));
            ELSEIF  v_op = 'Lte'         THEN SET v_cond = CONCAT('`', v_field, '` <= ', QUOTE(v_value));
            ELSEIF  v_op = 'Between'     THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
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
                SET v_cond = IF(v_in_list != '', CONCAT('`', v_field, IF(v_op = 'In', '` IN (', '` NOT IN ('), v_in_list, ')'), IF(v_op = 'In', '1 = 0', '1 = 1'));
            END IF;

            IF v_cond IS NOT NULL THEN
                IF v_where != '' THEN SET v_where = CONCAT(v_where, ' AND '); END IF;
                SET v_where = CONCAT(v_where, v_cond);
            END IF;
            SET v_i = v_i + 1;
        END WHILE;
    END IF;

    IF v_sort IS NOT NULL AND v_sort != '' THEN
        SET v_sort_count = 1 + (LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', '')));
        SET v_sort_i = 1; SET v_order_by = 'ORDER BY ';
        WHILE v_sort_i <= v_sort_count DO
            SET v_sort_part = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(v_sort, ',', v_sort_i), ',', -1));
            IF LEFT(v_sort_part, 1) = '-' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'DESC';
            ELSEIF LEFT(v_sort_part, 1) = '+' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'ASC';
            ELSE SET v_sort_field = v_sort_part; SET v_sort_dir = 'ASC';
            END IF;
            IF v_sort_i > 1 THEN SET v_order_by = CONCAT(v_order_by, ', '); END IF;
            SET v_order_by = CONCAT(v_order_by, '`', v_sort_field, '` ', v_sort_dir);
            SET v_sort_i = v_sort_i + 1;
        END WHILE;
    ELSE
        SET v_order_by = 'ORDER BY `DepartmentID` DESC';
    END IF;

    SET v_sql       = CONCAT('SELECT * FROM `Department`',       IF(v_where != '', CONCAT(' WHERE ', v_where), ''), ' ', v_order_by, ' LIMIT ', v_pageSize, ' OFFSET ', v_offset);
    SET v_count_sql = CONCAT('SELECT COUNT(*) FROM `Department`', IF(v_where != '', CONCAT(' WHERE ', v_where), ''));
    PREPARE stmt FROM v_sql;       EXECUTE stmt;       DEALLOCATE PREPARE stmt;
    PREPARE stmt FROM v_count_sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END$$


-- ============================================================
-- Position
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Position_AdvancedFilterPaging`$$
CREATE PROCEDURE `Proc_Position_AdvancedFilterPaging`(
    IN v_pageIndex INT,
    IN v_pageSize  INT,
    IN v_sort      VARCHAR(500),
    IN v_filters   JSON
)
BEGIN
    DECLARE v_offset INT;
    DECLARE v_i INT DEFAULT 0; DECLARE v_filter_count INT DEFAULT 0;
    DECLARE v_field VARCHAR(255) DEFAULT ''; DECLARE v_op VARCHAR(50) DEFAULT '';
    DECLARE v_value TEXT DEFAULT NULL; DECLARE v_value_to TEXT DEFAULT NULL;
    DECLARE v_cond TEXT DEFAULT NULL; DECLARE v_where TEXT DEFAULT '';
    DECLARE v_j INT DEFAULT 0; DECLARE v_in_count INT DEFAULT 0; DECLARE v_in_list TEXT DEFAULT '';
    DECLARE v_sort_count INT DEFAULT 0; DECLARE v_sort_i INT DEFAULT 1;
    DECLARE v_sort_part VARCHAR(255) DEFAULT ''; DECLARE v_sort_field VARCHAR(255) DEFAULT '';
    DECLARE v_sort_dir VARCHAR(10) DEFAULT 'ASC'; DECLARE v_order_by TEXT DEFAULT '';
    DECLARE v_sql TEXT; DECLARE v_count_sql TEXT;

    IF v_pageIndex IS NULL OR v_pageIndex < 1 THEN SET v_pageIndex = 1; END IF;
    IF v_pageSize  IS NULL OR v_pageSize  < 1 THEN SET v_pageSize  = 20; END IF;
    SET v_offset = (v_pageIndex - 1) * v_pageSize;

    IF v_filters IS NOT NULL AND JSON_LENGTH(v_filters) > 0 THEN
        SET v_filter_count = JSON_LENGTH(v_filters);
        WHILE v_i < v_filter_count DO
            SET v_field = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].field')));
            SET v_op    = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].operator')));
            SET v_cond  = NULL;
            SET v_value = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))));
            SET v_value_to = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))));

            IF      v_op = 'Eq'          THEN SET v_cond = CONCAT('`', v_field, '` = ', QUOTE(v_value));
            ELSEIF  v_op = 'Neq'         THEN SET v_cond = CONCAT('`', v_field, '` <> ', QUOTE(v_value));
            ELSEIF  v_op = 'Contains'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'NotContains' THEN SET v_cond = CONCAT('`', v_field, '` NOT LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'StartsWith'  THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'EndsWith'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ')');
            ELSEIF  v_op = 'Empty'       THEN SET v_cond = CONCAT('(`', v_field, '` IS NULL OR `', v_field, '` = '''')');
            ELSEIF  v_op = 'NotEmpty'    THEN SET v_cond = CONCAT('(`', v_field, '` IS NOT NULL AND `', v_field, '` <> '''')');
            ELSEIF  v_op = 'Gt'          THEN SET v_cond = CONCAT('`', v_field, '` > ', QUOTE(v_value));
            ELSEIF  v_op = 'Lt'          THEN SET v_cond = CONCAT('`', v_field, '` < ', QUOTE(v_value));
            ELSEIF  v_op = 'Gte'         THEN SET v_cond = CONCAT('`', v_field, '` >= ', QUOTE(v_value));
            ELSEIF  v_op = 'Lte'         THEN SET v_cond = CONCAT('`', v_field, '` <= ', QUOTE(v_value));
            ELSEIF  v_op = 'Between'     THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
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
                SET v_cond = IF(v_in_list != '', CONCAT('`', v_field, IF(v_op = 'In', '` IN (', '` NOT IN ('), v_in_list, ')'), IF(v_op = 'In', '1 = 0', '1 = 1'));
            END IF;

            IF v_cond IS NOT NULL THEN
                IF v_where != '' THEN SET v_where = CONCAT(v_where, ' AND '); END IF;
                SET v_where = CONCAT(v_where, v_cond);
            END IF;
            SET v_i = v_i + 1;
        END WHILE;
    END IF;

    IF v_sort IS NOT NULL AND v_sort != '' THEN
        SET v_sort_count = 1 + (LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', '')));
        SET v_sort_i = 1; SET v_order_by = 'ORDER BY ';
        WHILE v_sort_i <= v_sort_count DO
            SET v_sort_part = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(v_sort, ',', v_sort_i), ',', -1));
            IF LEFT(v_sort_part, 1) = '-' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'DESC';
            ELSEIF LEFT(v_sort_part, 1) = '+' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'ASC';
            ELSE SET v_sort_field = v_sort_part; SET v_sort_dir = 'ASC';
            END IF;
            IF v_sort_i > 1 THEN SET v_order_by = CONCAT(v_order_by, ', '); END IF;
            SET v_order_by = CONCAT(v_order_by, '`', v_sort_field, '` ', v_sort_dir);
            SET v_sort_i = v_sort_i + 1;
        END WHILE;
    ELSE
        SET v_order_by = 'ORDER BY `PositionID` DESC';
    END IF;

    SET v_sql       = CONCAT('SELECT * FROM `Position`',       IF(v_where != '', CONCAT(' WHERE ', v_where), ''), ' ', v_order_by, ' LIMIT ', v_pageSize, ' OFFSET ', v_offset);
    SET v_count_sql = CONCAT('SELECT COUNT(*) FROM `Position`', IF(v_where != '', CONCAT(' WHERE ', v_where), ''));
    PREPARE stmt FROM v_sql;       EXECUTE stmt;       DEALLOCATE PREPARE stmt;
    PREPARE stmt FROM v_count_sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END$$


-- ============================================================
-- Candidate
-- ============================================================
DROP PROCEDURE IF EXISTS `Proc_Candidate_AdvancedFilterPaging`$$
CREATE PROCEDURE `Proc_Candidate_AdvancedFilterPaging`(
    IN v_pageIndex INT,
    IN v_pageSize  INT,
    IN v_sort      VARCHAR(500),
    IN v_filters   JSON
)
BEGIN
    DECLARE v_offset INT;
    DECLARE v_i INT DEFAULT 0; DECLARE v_filter_count INT DEFAULT 0;
    DECLARE v_field VARCHAR(255) DEFAULT ''; DECLARE v_op VARCHAR(50) DEFAULT '';
    DECLARE v_value TEXT DEFAULT NULL; DECLARE v_value_to TEXT DEFAULT NULL;
    DECLARE v_cond TEXT DEFAULT NULL; DECLARE v_where TEXT DEFAULT '';
    DECLARE v_j INT DEFAULT 0; DECLARE v_in_count INT DEFAULT 0; DECLARE v_in_list TEXT DEFAULT '';
    DECLARE v_sort_count INT DEFAULT 0; DECLARE v_sort_i INT DEFAULT 1;
    DECLARE v_sort_part VARCHAR(255) DEFAULT ''; DECLARE v_sort_field VARCHAR(255) DEFAULT '';
    DECLARE v_sort_dir VARCHAR(10) DEFAULT 'ASC'; DECLARE v_order_by TEXT DEFAULT '';
    DECLARE v_sql TEXT; DECLARE v_count_sql TEXT;

    IF v_pageIndex IS NULL OR v_pageIndex < 1 THEN SET v_pageIndex = 1; END IF;
    IF v_pageSize  IS NULL OR v_pageSize  < 1 THEN SET v_pageSize  = 20; END IF;
    SET v_offset = (v_pageIndex - 1) * v_pageSize;

    IF v_filters IS NOT NULL AND JSON_LENGTH(v_filters) > 0 THEN
        SET v_filter_count = JSON_LENGTH(v_filters);
        WHILE v_i < v_filter_count DO
            SET v_field = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].field')));
            SET v_op    = JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].operator')));
            SET v_cond  = NULL;
            SET v_value = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].value'))));
            SET v_value_to = IF(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo')) IS NULL OR JSON_TYPE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))) = 'NULL', NULL, JSON_UNQUOTE(JSON_EXTRACT(v_filters, CONCAT('$[', v_i, '].valueTo'))));

            IF      v_op = 'Eq'          THEN SET v_cond = CONCAT('`', v_field, '` = ', QUOTE(v_value));
            ELSEIF  v_op = 'Neq'         THEN SET v_cond = CONCAT('`', v_field, '` <> ', QUOTE(v_value));
            ELSEIF  v_op = 'Contains'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'NotContains' THEN SET v_cond = CONCAT('`', v_field, '` NOT LIKE CONCAT(''%'', ', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'StartsWith'  THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(', QUOTE(v_value), ', ''%'')');
            ELSEIF  v_op = 'EndsWith'    THEN SET v_cond = CONCAT('`', v_field, '` LIKE CONCAT(''%'', ', QUOTE(v_value), ')');
            ELSEIF  v_op = 'Empty'       THEN SET v_cond = CONCAT('(`', v_field, '` IS NULL OR `', v_field, '` = '''')');
            ELSEIF  v_op = 'NotEmpty'    THEN SET v_cond = CONCAT('(`', v_field, '` IS NOT NULL AND `', v_field, '` <> '''')');
            ELSEIF  v_op = 'Gt'          THEN SET v_cond = CONCAT('`', v_field, '` > ', QUOTE(v_value));
            ELSEIF  v_op = 'Lt'          THEN SET v_cond = CONCAT('`', v_field, '` < ', QUOTE(v_value));
            ELSEIF  v_op = 'Gte'         THEN SET v_cond = CONCAT('`', v_field, '` >= ', QUOTE(v_value));
            ELSEIF  v_op = 'Lte'         THEN SET v_cond = CONCAT('`', v_field, '` <= ', QUOTE(v_value));
            ELSEIF  v_op = 'Between'     THEN SET v_cond = CONCAT('`', v_field, '` BETWEEN ', QUOTE(v_value), ' AND ', QUOTE(v_value_to));
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
                SET v_cond = IF(v_in_list != '', CONCAT('`', v_field, IF(v_op = 'In', '` IN (', '` NOT IN ('), v_in_list, ')'), IF(v_op = 'In', '1 = 0', '1 = 1'));
            END IF;

            IF v_cond IS NOT NULL THEN
                IF v_where != '' THEN SET v_where = CONCAT(v_where, ' AND '); END IF;
                SET v_where = CONCAT(v_where, v_cond);
            END IF;
            SET v_i = v_i + 1;
        END WHILE;
    END IF;

    IF v_sort IS NOT NULL AND v_sort != '' THEN
        SET v_sort_count = 1 + (LENGTH(v_sort) - LENGTH(REPLACE(v_sort, ',', '')));
        SET v_sort_i = 1; SET v_order_by = 'ORDER BY ';
        WHILE v_sort_i <= v_sort_count DO
            SET v_sort_part = TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(v_sort, ',', v_sort_i), ',', -1));
            IF LEFT(v_sort_part, 1) = '-' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'DESC';
            ELSEIF LEFT(v_sort_part, 1) = '+' THEN SET v_sort_field = SUBSTRING(v_sort_part, 2); SET v_sort_dir = 'ASC';
            ELSE SET v_sort_field = v_sort_part; SET v_sort_dir = 'ASC';
            END IF;
            IF v_sort_i > 1 THEN SET v_order_by = CONCAT(v_order_by, ', '); END IF;
            SET v_order_by = CONCAT(v_order_by, '`', v_sort_field, '` ', v_sort_dir);
            SET v_sort_i = v_sort_i + 1;
        END WHILE;
    ELSE
        SET v_order_by = 'ORDER BY `CandidateID` DESC';
    END IF;

    SET v_sql       = CONCAT('SELECT * FROM `Candidate`',       IF(v_where != '', CONCAT(' WHERE ', v_where), ''), ' ', v_order_by, ' LIMIT ', v_pageSize, ' OFFSET ', v_offset);
    SET v_count_sql = CONCAT('SELECT COUNT(*) FROM `Candidate`', IF(v_where != '', CONCAT(' WHERE ', v_where), ''));
    PREPARE stmt FROM v_sql;       EXECUTE stmt;       DEALLOCATE PREPARE stmt;
    PREPARE stmt FROM v_count_sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END$$

DELIMITER ;
