# BẢN ĐỒ KIẾN TRÚC QRDINE (BLUEPRINT)

Dự án dùng Onion Architecture + Vertical Slice. KHÔNG liệt kê chi tiết các file đã có. Dưới đây là KHUÔN MẪU chuẩn. Khi tạo module MỚI (vd: Inventory, Promotions...), BẮT BUỘC phải tuân thủ việc sinh file theo các layer sau.

**Onion Architecture - Dependencies Flow (BẮT BUỘC THEO QUY TẮC):**

**Quy tắc:**

- Domain: Độc lập, KHÔNG phụ thuộc layer nào.
- Application: CHỈ phụ thuộc Domain + Application.Common (abstractions).
- Infrastructure: Phụ thuộc Application + Application.Common + Domain.
- API: Phụ thuộc Infrastructure + Application + Application.Common + Domain.
- **Database:** KHÔNG phép inject `DbContext` vào Application → Dùng `IApplicationDbContext`.

## 1. Tầng Core: /QRDine.Domain

Nơi chứa Entities, không phụ thuộc framework ngoài.

- `/Common` -> BaseEntity, IMustHaveMerchant.
- `/{FeatureName}` -> Entities của feature đó (Ví dụ: `/Catalog`, `/Billing`, `/NewFeature`).
- `/Enums` & `/Constants` -> Dùng chung.

## 2. Tầng Hợp đồng: /QRDine.Application.Common

Chỉ chứa Interfaces & Base classes, KHÔNG có implementation.

- `/Abstractions/{Concern}` → ICacheService, IEmailService, IRepository, **IApplicationDbContext**, **IDatabaseTransaction**.
  - **IApplicationDbContext:** Abstract database connection (không dùng DbContext trực tiếp).
  - **IDatabaseTransaction:** Transaction interface để manual Commit/Rollback.
- `/Exceptions`, `/Models` (PagedResult), `/Constants`.

## 3. Tầng Nghiệp vụ: /QRDine.Application

Áp dụng CQRS + Vertical Slice.
Mỗi Feature MỚI phải có cấu trúc thư mục sau:

- `/Features/{FeatureName}/`
  - `/Repositories` -> Interface: `I{Entity}Repository` kế thừa `IRepository<{Entity}>`. Naming: `I{Entity}Repository.cs`.
  - `/Commands` -> Handler xử lý Create/Update/Delete.
  - `/Queries` -> Handler xử lý Get/List.
  - `/Validators` -> FluentValidation Validators (BẮT BUỘC).
  - `/DTOs` -> Data classes trả về.
  - `/Extensions` -> ExpressionTree mapping Entity → DTO (tránh N+1).
  - `/Services` -> Business Logic/Orchestration (Transaction, Validate chéo).
  - `/Specifications` -> EF Core query logic.

## 4. Tầng Thực thi: /QRDine.Infrastructure

Implement các interface từ Application.Common.

- `/Persistence`
  - **`ApplicationDbContext.cs`** → Implement `IApplicationDbContext, IAsyncDisposable`.
    - `BeginTransactionAsync()` → trả về `IDatabaseTransaction`.
    - `SaveChangesAsync()` → save changes.
  - `/Configurations/{FeatureName}` → EF Fluent API mapping cho entities.
  - `/Migrations` & `/Seeding`.
  - **DatabaseTransaction.cs** → Implement `IDatabaseTransaction` (wrapper cho IDbContextTransaction).
- `/{FeatureName}/Repositories` → Implement: `{Entity}Repository : Repository<{Entity}>, I{Entity}Repository`.
  - Constructor: Inject `ApplicationDbContext context` → gọi `base(context)`.
  - KHÔNG dùng null-check ở Repository (base Repository xử lý).
- `/Identity`, `/SignalR`, `/Email`, `/ExternalServices`,... → Các module cắm ngoài (Pluggable).

## 5. Tầng Giao tiếp: /QRDine.API

- `/Controllers` -> Chia theo đối tượng user:
  - `/Management/{FeatureName}` -> API cho Chủ quán (Yêu cầu Token).
  - `/Storefront/{FeatureName}` -> API cho Khách quét QR (Public).
  - `/Admin/{FeatureName}` -> API cho Super Admin.
- `/Requests/{FeatureName}` -> Input forms (CreateXForm, UpdateXForm).
- `/Filters`, `/Attributes`, `/Middlewares` -> Gating & Security.
- `/DependencyInjection` -> Nơi đăng ký Service (IoC), phân mảnh rõ ràng theo Layer/Concern:
  - `ServiceCollectionExtensions.cs` -> **(TRÁI TIM HỆ THỐNG - BẮT BUỘC CẬP NHẬT)**: Nơi gom toàn bộ DI. Khi đẻ thêm module, BẮT BUỘC phải mở file này ra để gọi method khai báo (Ví dụ: gọi `services.Add{FeatureName}Feature();` vào bên trong hàm `AddFeatures()`).\n - **Register Order:** Repositories → Services → Validators → Handlers → Profiles (AutoMapper).
  - `ApplicationBuilderExtensions.cs` -> **(TRÁI TIM PIPELINE & SEEDING)**: Nơi cấu hình Middleware pipeline và chạy khởi tạo dữ liệu (Ví dụ: Chèn lệnh gọi Seeder mới vào bên trong hàm `SeedDataAsync()`).
  - `/Features` -> (QUAN TRỌNG) Khi tạo Module mới, tạo file `{FeatureName}ServiceCollectionExtensions.cs` ở đây để đăng ký riêng rẽ (VD: Đăng ký Service, Policy của module đó).
  - `/Infrastructure` -> Đăng ký:
    - `services.AddScoped<ApplicationDbContext>();`
    - `services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());`
    - DB Context, Repositories, và External Services (Email, PayOS).
  - `/Application` -> Đăng ký MediatR, AutoMapper, FluentValidation.
  - `/Security` -> Đăng ký Authentication (JWT), Authorization, CORS.
  - `/Presentation` -> Đăng ký Controllers, Swagger, Filters, API Versioning.
  - `/CrossCutting` -> Đăng ký các service tiện ích toàn cục (Logging, Caching).

## 6. DEPENDENCY & LIBRARY CONSTRAINTS (LUẬT PHỤ THUỘC NẾU NHƯ TÍNH NĂNG MỚI CẦN PHẢI CÀI THÊM THƯ VIỆN)

Dự án hiện tại đang tuân thủ nghiêm ngặt Onion Architecture. Nếu như cần phải thêm thư viện cho tính năng mới thì cần đọc và hiểu rõ quy tắc dưới đây mới được thực hiện. Vi phạm sẽ gây lỗi Circular Dependency và rò rỉ Tech Stack:

- **Domain:** Độc lập 100%. KHÔNG phụ thuộc bất kỳ project hay thư viện ngoài nào.
- **Application (Lõi Nghiệp Vụ):** CHỈ phụ thuộc `Domain` + `Application.Common`.
  - **Được phép cài:** Thuần logic/pattern (`MediatR`, `FluentValidation`, `AutoMapper`, `Ardalis.Specification` hoặc các thư viện mang tính hỗ trợ luồng nghiệp vụ).
  - **CẤM TUYỆT ĐỐI:** Cài các thư viện công nghệ (`EF Core`, `SQL Server`, `ASP.NET Core`, `Cloudinary`, `MailKit`, `PayOS`, các thư viện mang tính công nghệ xử lý logic cấp cao).
  - **Cấm Inject:** Tuyệt đối KHÔNG inject `DbContext` vào tầng này -> BẮT BUỘC dùng `IApplicationDbContext` hoặc interface Repository.
- **Infrastructure (Chi Tiết Cài Đặt):** Phụ thuộc `Application` + `Common` + `Domain`.
  - **Được phép cài:** CHỨA TẤT CẢ các thư viện giao tiếp với thế giới bên ngoài: ORM (`EF Core`, `SQL Server`), Security (`JWT`, `Identity`), Cache (`Redis`), SDKs (`Cloudinary`, `MailKit`, `PayOS`,...).
- **API (Tầng Giao Tiếp):** Phụ thuộc `Infrastructure` + `Application` + `Common` + `Domain` để đăng ký DI Container và HTTP Pipeline.
