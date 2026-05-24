# Danh mục Thực đơn MISA CukCuk — Sơ đồ & Mô hình hóa

**Version:** 1.0  
**Ngày:** 18/05/2026  
**Công cụ:** Mermaid (render trong Markdown, GitHub, Notion, Confluence)

---

## 1. Use Case Diagram — Tổng quan tính năng hệ thống

> Mô tả toàn bộ chức năng của module và actor tương tác.

```mermaid
graph LR
    QL[👤 Quản lý\nnhà hàng]

    subgraph Module["Danh mục Thực đơn — MISA CukCuk"]
        UC1(Xem danh sách\nThực đơn)
        UC2(Tìm kiếm &\nLọc Thực đơn)
        UC3(Thêm mới\nThực đơn)
        UC4(Sửa\nThực đơn)
        UC5(Xóa\nThực đơn)
        UC6(Nhân bản\nThực đơn)
        UC7(Quản lý Sở thích\nphục vụ)
        UC8(Upload ảnh\nThực đơn)
        UC9(Ghim cột &\nLọc nhanh theo cột)
        UC10(Thêm nhanh Nhóm\nThực đơn / ĐVT)
    end

    QL --> UC1
    QL --> UC2
    QL --> UC3
    QL --> UC4
    QL --> UC5
    QL --> UC6
    QL --> UC7
    QL --> UC8
    QL -.->|Bonus| UC9
    QL -.->|Bonus| UC10

    UC3 -->|include| UC7
    UC3 -->|include| UC8
    UC4 -->|include| UC7
    UC4 -->|include| UC8
    UC6 -->|extend| UC3
```

---

## 2. ERD — Entity Relationship Diagram

> Cấu trúc database, quan hệ giữa các bảng.

```mermaid
erDiagram
    unit {
        char(36) UnitID PK
        varchar(255) UnitName "Unique, NOT NULL"
        text Description
        tinyint IsDeleted "Default 0"
    }

    kitchen {
        char(36) KitchenID PK
        varchar(255) KitchenName "NOT NULL"
        tinyint IsDeleted "Default 0"
    }

    inventory_item_category {
        char(36) CategoryID PK
        varchar(50) CategoryCode "Unique, NOT NULL"
        varchar(255) CategoryName "NOT NULL"
        tinyint MenuItemType "0=Món ăn|1=Đóng chai|2=Pha chế"
        tinyint IsDeleted "Default 0"
    }

    inventory_item_category_kitchen {
        char(36) CategoryID PK,FK
        char(36) KitchenID PK,FK
    }

    inventory_item {
        char(36) MenuItemID PK
        varchar(255) MenuItemCode "Unique, NOT NULL"
        varchar(255) MenuItemName "NOT NULL"
        tinyint MenuItemType "0=Món ăn|1|2"
        char(36) CategoryID FK
        char(36) UnitID FK "NOT NULL"
        decimal(18-2) SalePrice "NOT NULL, >=0"
        decimal(18-2) CostPrice "Default 0"
        varchar(500) Image
        varchar(500) Description
        tinyint IsDiscontinued "Default 0"
        tinyint IsDeleted "Default 0"
    }

    inventory_item_kitchen {
        char(36) MenuItemID PK,FK
        char(36) KitchenID PK,FK
    }

    inventory_item_addition {
        char(36) AdditionID PK
        varchar(255) AdditionName "NOT NULL"
        decimal(18-2) ExtraCharge "Default 0"
        tinyint IsDeleted "Default 0"
    }

    inventory_item_addition_detail {
        char(36) DetailID PK
        char(36) MenuItemID FK "NOT NULL"
        char(36) AdditionID FK "NOT NULL"
        decimal(18-2) ExtraCharge "Override từ master"
        int SortOrder "Default 0"
    }

    unit ||--o{ inventory_item : "UnitID (1 ĐVT - N món)"
    inventory_item_category ||--o{ inventory_item : "CategoryID (1 nhóm - N món)"
    inventory_item_category }o--o{ kitchen : "inventory_item_category_kitchen"
    inventory_item }o--o{ kitchen : "inventory_item_kitchen"
    inventory_item ||--o{ inventory_item_addition_detail : "MenuItemID (1 món - N sở thích)"
    inventory_item_addition ||--o{ inventory_item_addition_detail : "AdditionID (1 sở thích - N detail)"
```

---

## 3. Flowchart — Luồng xử lý Màn hình danh sách

> Mô tả quy trình từ khi vào trang đến các thao tác chính.

```mermaid
flowchart TD
    A([Người dùng vào\nmàn hình Thực đơn]) --> B[GET /InventoryItems/Paging\npageIndex=1 & pageSize=10]
    B --> C{API\nthành công?}
    C -->|Không| D[Toast: Có lỗi xảy ra] --> E([Kết thúc])
    C -->|Có| F[Render bảng dữ liệu\n+ footer phân trang]

    F --> G{Người dùng\nthao tác gì?}

    G -->|Nhập tìm kiếm| H[GET /Paging?search=xxx]
    H --> F

    G -->|Chọn trang/số dòng| I[GET /Paging?pageIndex=N\n&pageSize=M]
    I --> F

    G -->|Hover dòng| J[Hiện icon: Sửa/Nhân bản/Xóa]

    J -->|Click Sửa\nhoặc Double-click| K[GET /InventoryItems/id]
    K --> L[Mở Form Sửa]

    J -->|Click Nhân bản| M[POST /InventoryItems/id/clone]
    M --> N[Mở Form Nhân bản]

    J -->|Click Xóa| O[Hiện Modal xác nhận]
    O -->|Hủy| F
    O -->|Xác nhận| P[DELETE /InventoryItems/id]
    P --> Q[Toast: Xóa thành công]
    Q --> F

    G -->|Click Thêm| R[Mở Form Thêm mới]
```

---

## 4. Flowchart — Luồng xử lý Form Thêm / Sửa

> Quy trình nhập liệu và lưu dữ liệu.

```mermaid
flowchart TD
    A([Mở Form\nThêm/Sửa/Nhân bản]) --> B[Load dropdowns:\nGET /Units, /Kitchens,\n/InventoryItemCategories]
    B --> C[Render form\nAuto focus ô Tên món]

    C --> D{Người dùng\nthao tác}

    D -->|Upload ảnh| E[POST /upload-image]
    E --> F{Upload\nthành công?}
    F -->|Không| G[Toast lỗi upload] --> D
    F -->|Có| H[Hiện preview ảnh] --> D

    D -->|Tab sang tab\nSở thích phục vụ| I[GET /InventoryItemAdditions\nLoad dropdown sở thích]
    I --> J[Quản lý dòng sở thích\nclient-side] --> D

    D -->|Click Lưu\nhoặc Lưu và thêm| K{Validate FE}
    K -->|Fail| L[Focus input lỗi đầu tiên\nHiện tooltip lỗi]
    L --> D

    K -->|Pass| M{POST hoặc PUT\n/api/InventoryItems}
    M -->|400 lỗi BE| N[Toast lỗi từ userMessage] --> D
    M -->|500 server error| O[Toast: Có lỗi xảy ra] --> D
    M -->|201/200 OK| P[Toast thành công]

    P -->|Lưu| Q([Quay về danh sách])
    P -->|Lưu và thêm| R([Mở form trống mới])

    D -->|Click Hủy\nkhi có thay đổi| S[Modal: Bạn có thay đổi\nchưa lưu?]
    S -->|Hủy| D
    S -->|Đồng ý| Q
```

---

## 5. Sequence Diagram — Luồng Thêm mới Thực đơn

> Tương tác chi tiết giữa Frontend, Backend API và Database.

```mermaid
sequenceDiagram
    actor U as Quản lý nhà hàng
    participant FE as Frontend (Vue.js)
    participant BE as Backend (.NET 10)
    participant DB as MySQL Database

    U->>FE: Click "+ Thêm"

    par Load dropdown data
        FE->>BE: GET /api/Units
        BE->>DB: SELECT * FROM unit WHERE IsDeleted=0
        DB-->>BE: Danh sách đơn vị tính
        BE-->>FE: 200 OK [units]

        FE->>BE: GET /api/Kitchens
        BE->>DB: SELECT * FROM kitchen WHERE IsDeleted=0
        DB-->>BE: Danh sách bếp
        BE-->>FE: 200 OK [kitchens]

        FE->>BE: GET /api/InventoryItemCategories
        DB-->>BE: Danh sách nhóm thực đơn
        BE-->>FE: 200 OK [categories]
    end

    FE-->>U: Form trống mở, focus ô Tên món

    U->>FE: Nhập thông tin + chọn Sở thích phục vụ
    U->>FE: Click [Lưu]

    FE->>FE: Validate FE (required, format, maxlength)

    alt Validate FE fail
        FE-->>U: Focus input lỗi đầu tiên + tooltip lỗi
    else Validate FE pass
        FE->>BE: POST /api/InventoryItems {body}

        BE->>BE: ValidateCustom (business rules)
        BE->>DB: Kiểm tra trùng MenuItemCode
        DB-->>BE: Không trùng

        BE->>DB: CALL Proc_InsertInventoryItem(...)
        note over DB: INSERT inventory_item<br/>INSERT inventory_item_kitchen (mỗi bếp)<br/>INSERT inventory_item_addition_detail (mỗi sở thích)
        DB-->>BE: OK

        BE-->>FE: 201 Created {data: {menuItemID: "..."}}
        FE-->>U: Toast "Thêm thực đơn thành công"
        FE-->>U: Chuyển về danh sách, reload
    end
```

---

## 6. Sequence Diagram — Luồng Nhân bản Thực đơn

```mermaid
sequenceDiagram
    actor U as Quản lý nhà hàng
    participant FE as Frontend
    participant BE as Backend
    participant DB as MySQL Database

    U->>FE: Hover dòng → Click icon Nhân bản
    FE->>BE: POST /api/InventoryItems/{id}/clone

    BE->>DB: SELECT * FROM inventory_item WHERE MenuItemID = id
    DB-->>BE: Dữ liệu món gốc

    BE->>DB: SELECT * FROM inventory_item_kitchen WHERE MenuItemID = id
    DB-->>BE: Danh sách bếp

    BE->>DB: SELECT * FROM inventory_item_addition_detail WHERE MenuItemID = id
    DB-->>BE: Danh sách sở thích

    BE->>BE: Tự sinh MenuItemCode mới\n(kiểm tra không trùng trong DB)

    BE->>DB: CALL Proc_InsertInventoryItem(newCode, ...allData)
    note over DB: INSERT inventory_item (bản sao)<br/>INSERT inventory_item_kitchen (copy)<br/>INSERT inventory_item_addition_detail (copy)
    DB-->>BE: newMenuItemID

    BE-->>FE: 201 Created {data: {menuItemID: newId}}

    FE->>BE: GET /api/InventoryItems/{newId}
    BE->>DB: CALL Proc_InventoryItem_GetDetail(newId)
    DB-->>BE: Full data của bản sao
    BE-->>FE: 200 OK {data: {...}}

    FE-->>U: Form "Nhân bản thực đơn" mở với dữ liệu bản sao
```

---

## 7. State Diagram — Vòng đời của một Thực đơn

> Các trạng thái một món ăn có thể có trong hệ thống.

```mermaid
stateDiagram-v2
    [*] --> DangBan : Thêm mới thực đơn\n(IsDeleted=0, IsDiscontinued=0)

    DangBan --> DangBan : Sửa thông tin\n(PUT /InventoryItems)

    DangBan --> NgungBan : Tick checkbox "Ngừng bán"\n(IsDiscontinued=1)

    NgungBan --> DangBan : Bỏ tick "Ngừng bán"\n(IsDiscontinued=0)

    DangBan --> DaBan : Nhân bản\n(Tạo bản ghi mới)

    DangBan --> DaXoa : Xóa\n(IsDeleted=1, cascade detail)
    NgungBan --> DaXoa : Xóa\n(IsDeleted=1)

    DaXoa --> [*]

    note right of DangBan
        IsDeleted = 0
        IsDiscontinued = 0
    end note

    note right of NgungBan
        IsDeleted = 0
        IsDiscontinued = 1
    end note

    note right of DaXoa
        IsDeleted = 1
        Không hiển thị trong danh sách
    end note
```

---

## 8. Context Diagram — Góc nhìn tổng thể hệ thống

> Module Danh mục Thực đơn trong hệ sinh thái MISA CukCuk.

```mermaid
graph TD
    QL[👤 Quản lý\nnhà hàng] -->|Thêm/Sửa/Xóa Thực đơn| SYS((Module\nDanh mục\nThực đơn))
    SYS -->|Kết quả thao tác,\nThông báo lỗi/thành công| QL

    SYS -->|Query/Insert/Update/Delete\nqua Stored Procedures| DB[(MySQL\nDatabase)]

    SYS -->|Lưu ảnh món| FS[File Storage\n/uploads/inventory/]
    FS -->|URL ảnh| SYS

    SYS -.->|Dữ liệu thực đơn\ncho module bán hàng| BAN[Module\nBán hàng]

    STYLE[Style Guide\nMISA CukCuk] -.->|Icon, màu sắc,\ntypography| SYS
```

---

## 9. Swim Lane — Quy trình xử lý Xóa Thực đơn

> Phân rõ trách nhiệm giữa các thành phần.

```mermaid
flowchart LR
    subgraph USER [👤 Quản lý nhà hàng]
        U1[Hover dòng\nClick Xóa] --> U2[Nhìn thấy Modal\nxác nhận]
        U2 -->|Click Xác nhận| U3[Thao tác hoàn tất]
        U2 -->|Click Hủy| U4[Không xóa]
    end

    subgraph FE [Frontend Vue.js]
        F1[Hiển thị Modal:\nBạn có chắc chắn xóa?] --> F2{Người dùng\nxác nhận?}
        F2 -->|Có| F3[Gọi DELETE API]
        F2 -->|Không| F4[Đóng Modal]
        F3 --> F5[Hiện Toast\nthành công / lỗi]
        F5 --> F6[Reload danh sách]
    end

    subgraph BE [Backend .NET 10]
        B1[Nhận DELETE /id] --> B2[Validate: ID tồn tại?]
        B2 -->|Không| B3[Trả 404 Not Found]
        B2 -->|Có| B4[Gọi SP DeleteById]
        B4 --> B5[Trả 200 OK]
    end

    subgraph DB [MySQL Database]
        D1[Proc_DeleteInventoryItemById] --> D2[SET IsDeleted=1\ntrên inventory_item]
        D2 --> D3[DELETE inventory_item_kitchen]
        D3 --> D4[DELETE inventory_item_addition_detail]
    end

    U1 --> F1
    F3 --> B1
    B4 --> D1
    B5 --> F5
    B3 --> F5
```
