# Thiết kế Database

**Schema:** `misaemployee_development`  
**Engine:** MySQL 8+  
**Charset:** utf8mb4

---

## Sơ đồ quan hệ

```
unit ◄──────────────────────── inventory_item ──────────────► inventory_item_category
                                      │                                   │
                         inventory_item_kitchen            inventory_item_category_kitchen
                                      │                                   │
                                  kitchen ◄─────────────────────────────┘

inventory_item ──► inventory_item_addition_detail ──► inventory_item_addition
```

---

## 1. Bảng `unit` — Đơn vị tính

```sql
CREATE TABLE unit (
    UnitID       CHAR(36)     NOT NULL,
    UnitName     VARCHAR(255) NOT NULL COMMENT 'Tên đơn vị tính',
    Description  TEXT         NULL,
    CreatedBy    VARCHAR(255) NULL,
    CreateDate   DATETIME     NULL,
    ModifiedBy   VARCHAR(255) NULL,
    ModifiedDate DATETIME     NULL,
    State        INT          NULL,
    IsDeleted    TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (UnitID),
    CONSTRAINT UQ_UnitName UNIQUE (UnitName)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Dữ liệu mẫu:**

| UnitName |
|---|
| Suất |
| Bát |
| Đĩa |
| Cốc |
| Lon |
| Nồi |
| Tô |

---

## 2. Bảng `kitchen` — Bếp/Bar chế biến

```sql
CREATE TABLE kitchen (
    KitchenID    CHAR(36)     NOT NULL,
    KitchenName  VARCHAR(255) NOT NULL COMMENT 'Tên bếp/bar',
    Description  TEXT         NULL,
    CreatedBy    VARCHAR(255) NULL,
    CreateDate   DATETIME     NULL,
    ModifiedBy   VARCHAR(255) NULL,
    ModifiedDate DATETIME     NULL,
    State        INT          NULL,
    IsDeleted    TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (KitchenID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Dữ liệu mẫu:**

| KitchenName |
|---|
| Bếp |
| Bar |
| Lò |

---

## 3. Bảng `inventory_item_category` — Nhóm thực đơn

```sql
CREATE TABLE inventory_item_category (
    CategoryID   CHAR(36)     NOT NULL,
    CategoryCode VARCHAR(50)  NOT NULL COMMENT 'Mã nhóm, unique',
    CategoryName VARCHAR(255) NOT NULL COMMENT 'Tên nhóm',
    MenuItemType TINYINT      NOT NULL DEFAULT 0
        COMMENT '0=Món ăn | 1=Đồ uống đóng chai | 2=Đồ uống pha chế',
    Description  TEXT         NULL,
    CreatedBy    VARCHAR(255) NULL,
    CreateDate   DATETIME     NULL,
    ModifiedBy   VARCHAR(255) NULL,
    ModifiedDate DATETIME     NULL,
    State        INT          NULL,
    IsDeleted    TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (CategoryID),
    CONSTRAINT UQ_CategoryCode UNIQUE (CategoryCode)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 3.1 Bảng `inventory_item_category_kitchen` — Join nhóm ↔ bếp

```sql
CREATE TABLE inventory_item_category_kitchen (
    CategoryID CHAR(36) NOT NULL,
    KitchenID  CHAR(36) NOT NULL,
    PRIMARY KEY (CategoryID, KitchenID),
    CONSTRAINT FK_CatKitchen_Category
        FOREIGN KEY (CategoryID) REFERENCES inventory_item_category(CategoryID)
        ON DELETE CASCADE,
    CONSTRAINT FK_CatKitchen_Kitchen
        FOREIGN KEY (KitchenID) REFERENCES kitchen(KitchenID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

---

## 4. Bảng `inventory_item` — Thực đơn (entity chính)

```sql
CREATE TABLE inventory_item (
    -- PK
    MenuItemID        CHAR(36)      NOT NULL,

    -- Thông tin cơ bản
    MenuItemCode      VARCHAR(255)  NOT NULL  COMMENT 'Mã món, unique, max 255',
    MenuItemName      VARCHAR(255)  NOT NULL  COMMENT 'Tên món',
    MenuItemNameOther VARCHAR(255)  NULL      COMMENT 'Tên món theo ngôn ngữ khác, max 255',
    MenuItemType      TINYINT       NOT NULL DEFAULT 0
        COMMENT '0=Món ăn | 1=Đồ uống đóng chai | 2=Đồ uống pha chế',
    SortOrder         TINYINT       NOT NULL DEFAULT 0
        COMMENT '0=Món khai vị | 1=Món chính | 2=Món tráng miệng | 3=Món ăn vặt | 4=Món nước | 5=Món nướng | 6=Món lẩu',
    IsSignatureDish   TINYINT(1)    NOT NULL DEFAULT 0  COMMENT 'Là món đặc trưng',
    CategoryID        CHAR(36)      NULL      COMMENT 'FK inventory_item_category',
    UnitID            CHAR(36)      NOT NULL  COMMENT 'FK unit, bắt buộc',
    SalePrice         DECIMAL(18,2) NOT NULL DEFAULT 0  COMMENT 'Giá bán, bắt buộc',
    IsPriceChangeable TINYINT(1)    NOT NULL DEFAULT 0  COMMENT 'Thay đổi theo thời giá',
    IsPriceAdjustable TINYINT(1)    NOT NULL DEFAULT 0  COMMENT 'Điều chỉnh giá tự do',
    CostPrice         DECIMAL(18,2) NOT NULL DEFAULT 0  COMMENT 'Giá vốn',
    Image             VARCHAR(500)  NULL      COMMENT 'Đường dẫn file ảnh',
    Description       VARCHAR(500)  NULL      COMMENT 'Mô tả, max 500 ký tự',

    -- Thuế suất
    IndustryGroup     VARCHAR(255)  NULL      COMMENT 'Nhóm ngành nghề',
    VATRate           DECIMAL(5,2)  NULL      COMMENT 'Tỷ lệ tính thuế GTGT (%)',
    PITRate           DECIMAL(5,2)  NULL      COMMENT 'Tỷ lệ tính thuế TNCN (%)',
    IsVATReduced      TINYINT(1)    NOT NULL DEFAULT 1  COMMENT 'Món được giảm thuế GTGT',

    -- Thiết lập
    IsHiddenFromMenu          TINYINT(1) NOT NULL DEFAULT 0  COMMENT 'Không hiển thị trên thực đơn',
    IsSemiFinishedProduct     TINYINT(1) NOT NULL DEFAULT 0  COMMENT 'Là bán thành phẩm',
    IsAddedToOnlineMenu       TINYINT(1) NOT NULL DEFAULT 0  COMMENT 'Thêm vào thực đơn trang bán hàng Online',
    IsCopiedToOtherRestaurant TINYINT(1) NOT NULL DEFAULT 0  COMMENT 'Sao chép sang nhà hàng khác',
    IsDiscontinued            TINYINT(1) NOT NULL DEFAULT 0  COMMENT 'Ngừng bán',

    -- BaseModel
    CreatedBy    VARCHAR(255) NULL,
    CreateDate   DATETIME     NULL,
    ModifiedBy   VARCHAR(255) NULL,
    ModifiedDate DATETIME     NULL,
    State        INT          NULL,
    IsDeleted    TINYINT(1)   NOT NULL DEFAULT 0,

    PRIMARY KEY (MenuItemID),
    CONSTRAINT UQ_MenuItemCode UNIQUE (MenuItemCode),
    CONSTRAINT FK_Item_Category FOREIGN KEY (CategoryID)
        REFERENCES inventory_item_category(CategoryID),
    CONSTRAINT FK_Item_Unit FOREIGN KEY (UnitID)
        REFERENCES unit(UnitID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 4.1 Bảng `inventory_item_kitchen` — Join món ↔ bếp

```sql
CREATE TABLE inventory_item_kitchen (
    MenuItemID CHAR(36) NOT NULL,
    KitchenID  CHAR(36) NOT NULL,
    PRIMARY KEY (MenuItemID, KitchenID),
    CONSTRAINT FK_ItemKitchen_Item
        FOREIGN KEY (MenuItemID) REFERENCES inventory_item(MenuItemID)
        ON DELETE CASCADE,
    CONSTRAINT FK_ItemKitchen_Kitchen
        FOREIGN KEY (KitchenID) REFERENCES kitchen(KitchenID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

---

## 5. Bảng `inventory_item_addition` — Catalog sở thích phục vụ

> Đây là bảng **master** — danh sách sở thích toàn hệ thống. Dropdown "Sở thích phục vụ" load từ bảng này.

```sql
CREATE TABLE inventory_item_addition (
    AdditionID   CHAR(36)      NOT NULL,
    AdditionName VARCHAR(255)  NOT NULL  COMMENT 'Tên sở thích: không cay, ít đá...',
    ExtraCharge  DECIMAL(18,2) NOT NULL DEFAULT 0  COMMENT 'Phí thu thêm mặc định',
    CreatedBy    VARCHAR(255)  NULL,
    CreateDate   DATETIME      NULL,
    ModifiedBy   VARCHAR(255)  NULL,
    ModifiedDate DATETIME      NULL,
    State        INT           NULL,
    IsDeleted    TINYINT(1)    NOT NULL DEFAULT 0,
    PRIMARY KEY (AdditionID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Dữ liệu mẫu:**

| AdditionName | ExtraCharge |
|---|---|
| 20% đá | 0 |
| 50% đá | 0 |
| 125% size | 5000 |
| 20% đường | 0 |
| 50% đường | 0 |
| Thêm cá | 0 |
| Thêm bơ vừa | 0 |
| Không cay | 0 |
| Ít hành | 0 |

### 5.1 Bảng `inventory_item_addition_detail` — Sở thích gắn với từng món

> Ghi lại những sở thích nào được áp dụng cho 1 món cụ thể, với giá có thể override.

```sql
CREATE TABLE inventory_item_addition_detail (
    DetailID     CHAR(36)      NOT NULL,
    MenuItemID   CHAR(36)      NOT NULL,
    AdditionID   CHAR(36)      NOT NULL,
    ExtraCharge  DECIMAL(18,2) NOT NULL DEFAULT 0  COMMENT 'Giá thu thêm, override từ master',
    SortOrder    INT           NOT NULL DEFAULT 0,
    PRIMARY KEY (DetailID),
    CONSTRAINT FK_Detail_Item
        FOREIGN KEY (MenuItemID) REFERENCES inventory_item(MenuItemID)
        ON DELETE CASCADE,
    CONSTRAINT FK_Detail_Addition
        FOREIGN KEY (AdditionID) REFERENCES inventory_item_addition(AdditionID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

---

## Tóm tắt các bảng

| Bảng | Loại | Ghi chú |
|---|---|---|
| `unit` | Master | Đơn vị tính |
| `kitchen` | Master | Bếp chế biến |
| `inventory_item_category` | Master | Nhóm thực đơn |
| `inventory_item_category_kitchen` | Join | Nhóm ↔ Bếp (multi-select) |
| `inventory_item` | Chính | Thực đơn |
| `inventory_item_kitchen` | Join | Món ↔ Bếp (multi-select) |
| `inventory_item_addition` | Master | Catalog sở thích phục vụ |
| `inventory_item_addition_detail` | Detail | Sở thích gắn với từng món |

---

## Enum values

### MenuItemType
| Giá trị | Ý nghĩa |
|---|---|
| 0 | Món ăn |
| 1 | Đồ uống đóng chai |
| 2 | Đồ uống pha chế |

### SortOrder (Thứ tự món)
| Giá trị | Ý nghĩa |
|---|---|
| 0 | Món khai vị |
| 1 | Món chính |
| 2 | Món tráng miệng |
| 3 | Món ăn vặt |
| 4 | Món nước |
| 5 | Món nướng |
| 6 | Món lẩu |
