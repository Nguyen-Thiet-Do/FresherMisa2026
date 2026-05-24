# BA2 — Tài liệu Business Analyst
## Dự án: Danh mục Thực đơn MISA CukCuk

**Phiên bản:** 1.0 | **Ngày:** 18/05/2026 | **Nguồn:** Fresher MISA 2026

---

## Mục lục tài liệu

| File | Loại tài liệu | Mô tả |
|---|---|---|
| [01-BRD.md](./01-BRD.md) | BRD | Business Requirements Document — Yêu cầu nghiệp vụ tổng thể, phạm vi, stakeholder |
| [02-SRS.md](./02-SRS.md) | SRS | Software Requirements Specification — Yêu cầu kỹ thuật chi tiết (FR + NFR) |
| [03-user-stories.md](./03-user-stories.md) | User Stories | 15 User Stories với Acceptance Criteria theo format GIVEN/WHEN/THEN |
| [04-use-cases.md](./04-use-cases.md) | Use Case Specs | 8 Use Case Specification chi tiết: luồng chính, thay thế, ngoại lệ |
| [05-diagrams.md](./05-diagrams.md) | Sơ đồ | Use Case Diagram, ERD, Flowchart, Sequence Diagram, State Diagram, Swim Lane |

---

## Tóm tắt dự án

### Mục tiêu
Xây dựng module **Danh mục Thực đơn** cho hệ thống MISA CukCuk, cho phép nhà hàng quản lý toàn diện các món ăn và đồ uống: thêm, sửa, xóa, nhân bản, tìm kiếm, upload ảnh, gắn sở thích phục vụ.

### Tech Stack
- **Backend:** .NET 10, ASP.NET Core, Dapper, MySQL 8+
- **Database:** MySQL với Stored Procedures toàn bộ
- **Frontend:** Vue.js (theo đề bài)
- **API Base URL:** `http://localhost:5237/api`

### Các màn hình chính

```
┌─────────────────────────────────────────────────────────┐
│  Màn hình danh sách Thực đơn                            │
│  ✓ Phân trang (10/25/50/100 dòng)                       │
│  ✓ Tìm kiếm theo Mã món / Tên món                       │
│  ✓ Lọc theo Loại món                                    │
│  ✓ Hover actions: Sửa | Nhân bản | Xóa                  │
│  ✓ Double-click → mở form Sửa                           │
│  ★ Bonus: Ghim cột, lọc nhanh theo cột                  │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  Form Thêm / Sửa / Nhân bản                             │
│  ✓ Tab Thông tin chung (bắt buộc)                       │
│    - Thông tin cơ bản, Thuế suất, Thiết lập             │
│    - Upload ảnh                                         │
│  ✓ Tab Sở thích phục vụ (bắt buộc)                      │
│    - Thêm/xóa dòng sở thích                             │
│  ✓ Validate FE + BE                                     │
│  ✓ Auto focus, Tab navigation                           │
│  ★ Bonus: Thêm nhanh Nhóm TD / ĐVT                      │
└─────────────────────────────────────────────────────────┘
```

### Các bảng Database

| Bảng | Vai trò |
|---|---|
| `inventory_item` | Entity chính — Thực đơn |
| `inventory_item_category` | Master — Nhóm thực đơn |
| `unit` | Master — Đơn vị tính |
| `kitchen` | Master — Bếp/Bar chế biến |
| `inventory_item_kitchen` | Join — Món ↔ Bếp (nhiều-nhiều) |
| `inventory_item_addition` | Master — Catalog sở thích phục vụ |
| `inventory_item_addition_detail` | Detail — Sở thích gắn với từng món |
| `inventory_item_category_kitchen` | Join — Nhóm ↔ Bếp (nhiều-nhiều) |

### Bảng đánh giá (từ đề bài)

| Hạng mục | Trọng số |
|---|---|
| **Giao diện (UI/UX)** | **50%** |
| — Tuân thủ Style Guide | 30% |
| — Responsive ≥ 1366px | 5% |
| — Font chữ và tài nguyên | 5% |
| — Định dạng hiển thị dữ liệu | 5% |
| — Hiển thị đầy đủ Modal | 5% |
| **Chức năng** | **50%** |
| — CRUD | 20% |
| — Validate FE + BE | 15% |
| — Phân trang và hiển thị | 5% |
| — Tìm kiếm và lọc | 5% |
| — Điều hướng bằng phím | 5% |

---

## Cách đọc tài liệu này

Nếu bạn là **Developer:** Đọc theo thứ tự 02-SRS → 04-Use Cases → 05-Diagrams (ERD + Sequence).

Nếu bạn là **QA Tester:** Đọc 03-User Stories (Acceptance Criteria) + 04-Use Cases (Exception Flows).

Nếu bạn là **Mentor/Evaluator:** Đọc 01-BRD (phạm vi, stakeholder) + 05-Diagrams (overview).
