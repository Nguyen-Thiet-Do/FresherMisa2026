-- Audit Log Migration
-- Chạy trong schema: amis_tien_luong
USE amis_tien_luong;

CREATE TABLE IF NOT EXISTS audit_log (
    AuditLogID  CHAR(36)     NOT NULL PRIMARY KEY,
    EntityType  VARCHAR(100) NOT NULL COMMENT 'Tên bảng DB (e.g. pa_salary_composition, employee)',
    EntityId    CHAR(36)     NOT NULL COMMENT 'Khóa chính của bản ghi được thao tác',
    Action      TINYINT      NOT NULL COMMENT '1=Insert 2=Update 3=Delete',
    ActionBy    VARCHAR(255) NULL     COMMENT 'Username người thực hiện (null khi chưa có auth)',
    ActionAt    DATETIME     NOT NULL COMMENT 'Thời điểm thực hiện',
    INDEX idx_entity (EntityType, EntityId),
    INDEX idx_at    (ActionAt)
);
