# QUY TẮC & PATTERNS KIẾN TRÚC QRDINE

**Bộ Não Định Hướng** Bắt buộc sử dụng nó để suy nghĩ, nó không chỉ nơi đặt file. Bắt buộc dùng cùng `project_map.md`.

---

## FLOW CƠ BẢN: VERTICAL SLICE ARCHITECTURE

Khi nhận requirement, AI **BẮT BUỘC** phải phân loại trước khi code:

- **Trường hợp A (Tính năng mới trên Module ĐÃ CÓ - VD: API Xem Order):** Bước 1 & 2 chỉ dùng để ĐỌC và HIỂU concept. KHÔNG tạo file mới.
- **Trường hợp B (Module MỚI HOÀN TOÀN - VD: Quản lý Kho):** Thực hiện khởi tạo file từ đầu ở cả 5 bước.

### **Bước 1: DOMAIN (Core Entities)**

`src/QRDine.Domain/{FeatureName}/`

- **[Nếu Module Cũ]:** Đọc `{Entity}.cs` hiện có để hiểu cấu trúc field, relations.
- **[Nếu Module Mới]:** Tạo mới `{Entity}.cs`, Enum, Constants, kế thừa `BaseEntity.cs`.
  - **Bắt buộc check:** Entity có thuộc 1 Cửa hàng không? Nếu CÓ → Implement `IMustHaveMerchant`.

### **Bước 2: DATABASE CONFIGURATION**

`src/QRDine.Infrastructure/Persistence/Configurations/{FeatureName}/`

- **[Nếu Module Cũ]:** Đọc `{Entity}Configuration.cs` để nắm Index, composite keys đang có.
- **[Nếu Module Mới]:** Tạo `{Entity}Configuration.cs` (EF Fluent API).
  - Bắt buộc mở `ApplicationDbContext.cs` -> Đăng ký vào hàm `OnModelCreating()` và khai báo `DbSet<{Entity}>`.

### **Bước 3: REPOSITORY INTERFACES & CQRS**

**Repository Layer:**

`src/QRDine.Application/Features/{FeatureName}/Repositories/`

- Tạo `I{Entity}Repository.cs` - kế thừa `IRepository<{Entity}>`.
- Implement tại `src/QRDine.Infrastructure/{FeatureName}/Repositories/{Entity}Repository.cs`.
- **Lưu ý:** Repository chỉ chứa custom queries nếu cần. Nếu đơn giản, interface rỗng cũng được (dùng base Repository).

**APPLICATION CQRS:**

`src/QRDine.Application/Features/{FeatureName}/`

- **Mutations:** `/{Entity}/Commands/` -> Create/Update/Delete (Command + Handler).
- **Reads:** `/{Entity}/Queries/` -> GetList/GetDetail (Query + Handler).
- **Support:** `/{Entity}/DTOs/` và `/{Entity}/Mappings/` (Lưu ý: Mapping chỉ nên dùng cho Command, còn Query thì chủ yếu sử dụng Specification + Extension rồi nên để đảm bảo tính tối ưu thì sẽ không sử dụng mapping cũng được tránh select full bảng).
  _(Lưu ý Exceptions CQRS: Dashboards/Reports chỉ dùng Queries, Một số những trường hợp phức tạp thì nên tách ra service để xử lý tránh tình trạng handle phải làm quá nhiều việc gây vỡ kiến trúc)._

### **Bước 4: QUERY OPTIMIZATION (Specifications)**

`src/QRDine.Application/Features/{FeatureName}/{Entity}/Specifications/`

- Tạo `{Action}{Entity}Spec.cs` (VD: `GetActiveOrdersSpec.cs`).
- Đóng gói toàn bộ logic `.Where()`, `.Include()`, `.OrderBy()` vào đây bằng pattern ExpressionTree + Specification.
- **Khi nào dùng:** Query có >1 Include hoặc >1 Where condition. KHÔNG dùng cho single-table query quá đơn giản.

### **Bước 5: API LAYER & CHỐT SỔ HỆ THỐNG**

`src/QRDine.API/Controllers/Management/{FeatureName}/`

- Tạo `{Entity}Controller.cs` (Inject MediatR, gọi Commands/Queries).
- Tạo file Requests: `src/QRDine.API/Requests/{FeatureName}/Create{Entity}Form.cs`.
- **Security:** Gắn `[CheckFeatureLimit(FeatureType.{Feature})]` (nếu là Pro-only) hoặc `[SkipSubscriptionCheckAttribute]` (nếu là Public).

**ĐĂNG KÝ HỆ THỐNG (BẮT BUỘC KHÔNG ĐƯỢC QUÊN):**

1. **Tạo DI Module:** Tạo file `src/QRDine.API/DependencyInjection/Features/{FeatureName}ServiceCollectionExtensions.cs`.
2. **Khai báo DI Tổng:** MỞ FILE `ServiceCollectionExtensions.cs` -> Gắn hàm đăng ký của module vừa tạo vào bên trong method `AddFeatures()`.
3. **Khai báo Middleware/Seeding:** Nếu module mới cần nạp dữ liệu mẫu lúc startup hoặc có Middleware riêng, BẮT BUỘC mở file `ApplicationBuilderExtensions.cs` và thêm logic vào bên trong method `SeedDataAsync()`.

---

## 0. ONION ARCHITECTURE - DEPENDENCIES RULES (BẮTN BUỘC)

**Quy tắc phụ thuộc 1 chiều (TUYỆT ĐỐI KHÔNG VI PHẠM):**

```
Domain (độc lập) ← Application ← Infrastructure ← API
```

**Chi tiết:**

- **Domain:** KHÔNG phụ thuộc thằng nào. Chỉ định nghĩa Entities, Enums, Constants.
- **Application:** Chỉ phụ thuộc Application.Common + Domain. KHÔNG import Infrastructure hoặc API.
- **Infrastructure:** Phụ thuộc Application + Application.Common + Domain. Implement Repositories, Services, DB Context.
- **API:** Phụ thuộc Infrastructure + Application + Application.Common + Domain. Khởi tạo DI, Controllers.

**Database Access Rule (Cực kỳ quan trọng):**

- **KHÔNG ĐƯỢC** inject `DbContext` trực tiếp vào Handler/Service ở tầng Application.
- **BẮT BUỘC** dùng `IApplicationDbContext` (abstract interface ở Application.Common).
- **Transaction:** Dùng `IApplicationDbContext.BeginTransactionAsync()` + `IDatabaseTransaction`.
- **SaveChanges:** Gọi qua `IApplicationDbContext.SaveChangesAsync()`.

**VÍ DỤ SAI (CẤM TUYỆT ĐỐI):**

- Inject trực tiếp class `ApplicationDbContext` (của tầng Infrastructure) vào constructor của Handler.

**VÍ DỤ ĐÚNG (BẮT BUỘC):**

- **Injection:** Chỉ được inject interface `IApplicationDbContext` (thuộc `Application.Common`) và các interface service. Phải kiểm tra Null (`?? throw new ArgumentNullException`) ngay trong Constructor.
- **Transaction:** Mở transaction bằng `BeginTransactionAsync` (dùng `await using var`) ở ngay đầu hàm `Handle`.
- **Khối Try:** Thực thi logic (gọi service), lưu thay đổi (`SaveChangesAsync()`) và chốt transaction (`CommitAsync()`) bên trong khối `try`.
- **Khối Catch:** BẮT BUỘC gọi `RollbackAsync()` trước khi `throw;` để nhả lỗi ra ngoài (Middleware sẽ tự bắt).

---

## 1. CQRS, VERTICAL SLICE & DI CONVENTION

Mỗi Feature độc lập (có thể gọi chéo Repository miễn sao tuân thủ chuẩn nghiệp vụ). Luôn tuân thủ quy trình:

- **Mutations (Create/Update/Delete):** `Command + Handler`
- **Reads (Get/List):** `Query + Handler`
- **Repositories:** `I{Entity}Repository` (Application Layer) → `{Entity}Repository` (Infrastructure Layer).
- **Dependency Injection (DI):** Luôn tạo Interface → Implement → Đăng ký.

**QUY TẮC CONSTRUCTOR INJECTION (BẮT BUỘC KHÔNG BỎ SÓT):**
Mọi Handler và Service phải tuân thủ nghiêm ngặt tiêu chuẩn khởi tạo sau:

- **Naming Convention:** Field nội bộ phải dùng `private readonly` với tiền tố `_camelCase` (VD: `_merchantRepository`). Tham số truyền vào dùng `camelCase`.
- **Thứ tự Inject (Order):** `Repositories` -> `Services` -> `IApplicationDbContext` (nếu cần Transaction) -> `IMapper` (nếu có).
- **Phòng vệ (Null-Check):** BẮT BUỘC phải check Null cho TỪNG tham số ngay bên trong Constructor (dùng cú pháp `?? throw new ArgumentNullException(nameof(...))`).
- **Luật cấm Database Context:** TUYỆT ĐỐI KHÔNG inject trực tiếp các class `DbContext` hoặc `ApplicationDbContext`. Chỉ được phép gọi thông qua Interface `IApplicationDbContext` (thuộc `Application.Common.Abstractions.Persistence`).

**NGOẠI LỆ BẮT BUỘC:**

- `Dashboards` & `Reports`: **Chỉ Queries** (Thuần Analytics, cấm sinh Commands).
- `External Services` (Email, PayOS,...): Phải có 2 Impl (Prod/Fallback) + Config Settings.

---

## 2. DATA ACCESS, MULTI-TENANCY & PAGINATION

- **Repository Location:**
  - **Interface:** `src/QRDine.Application/Features/{FeatureName}/Repositories/I{Entity}Repository.cs`
  - **Implementation:** `src/QRDine.Infrastructure/{FeatureName}/Repositories/{Entity}Repository.cs`
  - **Base Class:** Kế thừa `Repository<{Entity}>` từ Infrastructure.
- **Cách ly Dữ liệu:** Entity của Cửa hàng BẮT BUỘC kế thừa `IMustHaveMerchant`. Dev **KHÔNG ĐƯỢC** viết `.Where(x => x.MerchantId == ...)` ở Repository/Handler.
- **DTOs Mapping Policy:**
  - **Command/Mutation:** Dùng `IMapper.Map()` (AutoMapper) → OK.
  - **Query từ DB:** Dùng `Expression<Func<Entity, Dto>>` (ExpressionTree) → BẮT BUỘC để tối ưu SQL.
  - **Khi nào dùng AutoMapper:** Chỉ khi logic đơn giản hoặc không từ DB (VD: map internal objects).
- **ExpressionTree Extensions (`/Extensions/`):** Mapping Entity → DTO trực tiếp bằng SQL. Viết property `Expression<Func<Entity, Dto>>` (VD: `ToOrderListDto`). Cấm dùng AutoMapper cho DB Query.
- **Specifications (`/Specifications/`):**
  - **Lấy Detail (1 record):** Kế thừa `Specification<Entity, Dto>` VÀ `ISingleResultSpecification<Entity, Dto>`. Bắt buộc gọi `Query.Select(Ext.ToDto)`.
  - **Lấy List (BẮT BUỘC PHÂN TRANG - DUAL SPEC):** Khi viết Query lấy danh sách, phải tuân thủ chuẩn sau:
    1. **Query Model:** Phải kế thừa `PaginationRequest` và trả về `PagedResult<Dto>` (hoặc `CursorPagedResult<Dto>`).
    2. **Đếm tổng (Count Spec):** Kế thừa `Specification<Entity>`. Chỉ chứa các điều kiện `.Where()`.
    3. **Lấy Data (ByPage Spec):** Kế thừa `Specification<Entity, Dto>`. Chứa nguyên xi các `.Where()` của Count Spec + `OrderBy` + `.Skip().Take()` + `Query.Select(Ext.ToDto)`.

---

## 3. SECURITY, API & REAL-TIME

- **Gating API:** - Public API: `[SkipSubscriptionCheckAttribute]`.
  - Pro-only API: `[CheckFeatureLimit(FeatureType.{Feature})]`.
- **Chuẩn Output:** Handler throw Custom Exceptions (`NotFoundException`, `BadRequestException`...). Middleware tự bắt và bọc vào chuẩn `{ isSuccess, data, meta, errors }`.
- **Real-Time:** Dùng SignalR `OrderHub` (Group theo `TableId`) và `OrderNotificationService` để broadcast.

---

## 4. CÁC LỖI THƯỜNG GẶP & CÁCH FIX (AI CẦN LƯU Ý KỸ)

**Lỗi 1: Lỗi Runtime DI (No service registered...)**

- **Nguyên nhân:** Tạo file mới (Service/Repo/Controller) nhưng quên chốt sổ IoC Container.
- **Cách khắc phục:** BẮT BUỘC mở `ServiceCollectionExtensions.cs` thêm khai báo vào `AddFeatures()`. Nếu có Seeding/Middleware thì mở `ApplicationBuilderExtensions.cs` để thêm.

**Lỗi 2: Phân trang sai (Thiếu data / Lệch TotalCount)**

- **Nguyên nhân:** Viết logic `.Where()` ở `CountSpec` và `ByPageSpec` không giống hệt nhau.
- **Cách khắc phục:** Khi viết Dual-Spec, phải đảm bảo copy chính xác 100% các khối điều kiện (như `if (!string.IsNullOrEmpty(searchTerm))`) từ CountSpec sang ByPageSpec.

**Lỗi 3: Lỗi EF Core Skip/Take (SQL Exception)**

- **Nguyên nhân:** Trong `ByPageSpec` gọi `.Skip()` và `.Take()` nhưng quên gọi hàm sắp xếp trước đó.
- **Cách khắc phục:** BẮT BUỘC phải gọi `Query.OrderBy(...).ThenBy(...)` TRƯỚC KHI gọi `.Skip().Take()`.

**Lỗi 4: Logic Handler phình to (Fat Handler)**

- **Nguyên nhân:** Nhét validate chéo, transaction database, hoặc logic tạo code tự động vào chung một CommandHandler.
- **Cách khắc phục:** Tách logic đó ra một `Interface/Class` riêng, đặt tại thư mục `/Services/` của Feature đó, rồi Inject service này vào Handler.

**Lỗi 5: Leak dữ liệu chéo Cửa hàng (Lỗi bảo mật nghiêm trọng)**

- **Nguyên nhân:** Entity mới quên kế thừa interface `IMustHaveMerchant`.
- **Cách khắc phục:** Sửa lại file Entity. (Hệ thống Middleware sẽ tự động chặn lỗi rò rỉ nhờ dấu hiệu này).

**Lỗi 6: Chậm DB do N+1 Query**

- **Nguyên nhân:** Gọi dữ liệu List lên RAM rồi mới dùng vòng lặp hoặc hàm C# để `.Select()`.
- **Cách khắc phục:** Ép logic mapping xuống tận SQL Server bằng cách gọi `Query.Select(...)` trỏ tới các hàm ExpressionTree trong file `...Extensions.cs`.

**Lỗi 7: Vi phạm Onion Architecture (Inject DbContext trực tiếp)**

- **Nguyên nhân:** Inject `DbContext`, `ApplicationDbContext` hoặc EF Core DbContext vào Handler/Service ở tầng Application.
- **Cách khắc phục:**
  - TUYỆT ĐỐI KHÔNG inject `DbContext` hay `ApplicationDbContext`.
  - Luôn dùng `IApplicationDbContext` từ `Application.Common.Abstractions.Persistence`.
  - Đảm bảo DI registration: `services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());`

---

## 5. TỪ ĐIỂN TÌM KIẾM NHANH (QUYẾT ĐỊNH HÀNH ĐỘNG)

- **Khi cần Mutation (Ghi/Xóa/Sửa):** Tạo `Command + Handler` tại thư mục `/Commands/`. Luôn trả về Dto hoặc Result.
- **Khi cần Lấy Danh sách (List):** Tạo `Query + Handler` VÀ **2 Specs** (CountSpec & ByPageSpec). Bắt buộc kế thừa `PaginationRequest` và trả về `PagedResult<Dto>`.
- **Khi cần Lấy Chi tiết (GetById):** Tạo `Query + Handler` VÀ **1 Spec**. Phải gắn thêm interface `ISingleResultSpecification` vào class Spec đó.
- **Khi cần Lọc dữ liệu / Mapping DB:** Viết logic vào file `/Extensions/{Entity}Extensions.cs` bằng kỹ thuật ExpressionTree để tái sử dụng.
- **Khi làm Báo cáo, Thống kê:** CHỈ tạo `Queries` (Nghiêm cấm sinh Command). Đặt code tại thư mục `/Dashboards/` hoặc `/Reports/`.
- **Khi cần Giới hạn quyền theo gói Pro:** Thêm attribute `[CheckFeatureLimit(FeatureType.X)]` trực tiếp lên Action trong Controller.
- **Khi viết API cho Khách không cần Login:** Thêm attribute `[SkipSubscriptionCheckAttribute]` trực tiếp lên Action trong Controller.

---

## 6. VALIDATORS & INPUT VALIDATION

- **Vị trí:** `src/QRDine.Application/Features/{FeatureName}/{Entity}/Validators/`
- **Naming:** `{Command/Query}Validator.cs` (VD: `CreateCategoryCommandValidator.cs`).
- **Rule:** Kế thừa `AbstractValidator<T>` (FluentValidation).
- **Scope:** Validate đầu vào (required, length, format, business rule).
- **Registration:** MediatR Behavior tự gọi qua Pipeline → KHÔNG cần đăng ký riêng.
- **Lưu ý:** Validator KHÔNG check quyền hay lookup DB (để cho Handler) → chỉ validate Data Structure.

---

## Chú ý Cuối Cùng

- **Bước 1 (Vertical Slice Flow) là CÔNG THỨC** → Dùng cho mọi feature mới
- **Decision Matrix là bút chì** → Tham khảo khi nghi vấn
- **project_map.md là BẢN ĐỒ** → Biết đặt file ở đâu
- **architecture_rules.md là LUẬT LỤC** → Biết suy nghĩ sao

**Status**: Active, maintained by Architecture Board  
**Last Updated**: April 2026
