# Catalog Module

The Catalog module manages the core menu structure for each merchant: **categories**, **products**, **topping groups**, **toppings**, and **restaurant tables**.

---

## Domain Entities

### Category

**File:** `src/QRDine.Domain/Catalog/Category.cs`

Represents a menu category with support for **1-level parent-child hierarchy** (e.g., "Drinks" → "Hot Drinks").

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Inherited from `BaseEntity` |
| `MerchantId` | `Guid` | Tenant scope (via `IMustHaveMerchant`) |
| `Name` | `string` | Category name (max 100 chars) |
| `Description` | `string?` | Optional description |
| `DisplayOrder` | `int` | Sort order within siblings (default: 0) |
| `IsActive` | `bool` | Active/inactive toggle (default: true) |
| `ParentId` | `Guid?` | Reference to parent category (`null` = root) |

**Navigation properties:** `Parent`, `Merchant`, `Children`, `Products`

### Product

**File:** `src/QRDine.Domain/Catalog/Product.cs`

A menu item belonging to a sub-category. Products **cannot** be assigned to root categories.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Inherited from `BaseEntity` |
| `MerchantId` | `Guid` | Tenant scope |
| `CategoryId` | `Guid` | Required parent category |
| `Name` | `string` | Product name (max 256 chars) |
| `Description` | `string?` | Optional description |
| `ImageUrl` | `string?` | Cloudinary image URL |
| `Price` | `decimal` | Unit price (≥ 0) |
| `IsAvailable` | `bool` | Availability flag (default: true) |

**Navigation properties:** `Merchant`, `Category`, `ProductToppingGroups`

### Table

**File:** `src/QRDine.Domain/Catalog/Table.cs`

Represents a physical restaurant table with a unique QR code token.

| Property | Type | Description |
|----------|------|-------------|
| `MerchantId` | `Guid` | Tenant scope |
| `Name` | `string` | Table identifier (e.g., "Table 1") |
| `IsOccupied` | `bool` | Occupancy status (default: false) |
| `QrCodeToken` | `string?` | Unique token for QR code generation |

### ToppingGroup

**File:** `src/QRDine.Domain/Catalog/ToppingGroup.cs`

A named group of toppings that can be attached to products (e.g., "Extra Shots", "Milk Options").

| Property | Type | Description |
|----------|------|-------------|
| `MerchantId` | `Guid` | Tenant scope |
| `Name` | `string` | Group name |
| `Description` | `string?` | Optional description |
| `IsRequired` | `bool` | Whether selection is mandatory (default: false) |
| `MinSelections` | `int` | Minimum selections required (default: 0) |
| `MaxSelections` | `int` | Maximum selections allowed (default: 1) |
| `IsActive` | `bool` | Active toggle |

**Navigation properties:** `Merchant`, `Toppings`, `ProductToppingGroups`

### Topping

**File:** `src/QRDine.Domain/Catalog/Topping.cs`

An individual topping item within a group.

| Property | Type | Description |
|----------|------|-------------|
| `ToppingGroupId` | `Guid` | Parent topping group |
| `Name` | `string` | Topping name |
| `Price` | `decimal` | Additional price (default: 0) |
| `DisplayOrder` | `int` | Sort order |
| `IsAvailable` | `bool` | Availability flag |

### ProductToppingGroup

**File:** `src/QRDine.Domain/Catalog/ProductToppingGroup.cs`

Many-to-many join entity linking products to topping groups.

| Property | Type |
|----------|------|
| `ProductId` | `Guid` |
| `ToppingGroupId` | `Guid` |

---

## CQRS: Commands & Queries

### Categories

#### CreateCategory

**Command:** `src/QRDine.Application/Features/Catalog/Categories/Commands/CreateCategory/CreateCategoryCommand.cs`  
**Handler:** `src/QRDine.Application/Features/Catalog/Categories/Commands/CreateCategory/CreateCategoryCommandHandler.cs`

**Endpoint:** `POST /api/v1/management/categories`  
**Auth:** `Merchant` role required  
**Input:** `CreateCategoryDto` (`Name`, `Description`, `DisplayOrder`, `ParentId`)  
**Output:** `CategoryResponseDto`

**Business rules:**
1. **1-level hierarchy limit** — If `ParentId` is specified, the parent must be a root category (its own `ParentId` must be `null`). Attempting to create a grandchild throws `BusinessRuleException`.
2. **Duplicate name prevention** — A category name must be unique within the same merchant, enforced by `CategoryByNameSpec`.
3. **Display order management** — If `DisplayOrder > 0`, existing sibling categories at or above that position are shifted up by 1 (via `ShiftDisplayOrdersAsync`). If `DisplayOrder == 0`, the category is appended at the end (`GetMaxDisplayOrderAsync + 1`).
4. **Transactional** — Display order shift and category creation are wrapped in a database transaction.

#### UpdateCategory

**Command:** `src/QRDine.Application/Features/Catalog/Categories/Commands/UpdateCategory/UpdateCategoryCommand.cs`  
**Handler:** `src/QRDine.Application/Features/Catalog/Categories/Commands/UpdateCategory/UpdateCategoryCommandHandler.cs`

**Endpoint:** `PUT /api/v1/management/categories/{id}`  
**Auth:** `Merchant` role required  
**Input:** Route `id` + `UpdateCategoryDto` (`Name`, `Description`, `DisplayOrder`, `IsActive`, `ParentId`)  
**Output:** `CategoryResponseDto`

**Business rules:**
1. **Self-parenting prevention** — A category cannot be its own parent.
2. **No moving categories with children** — A category that already has sub-categories cannot be moved under another parent.
3. **1-level hierarchy limit** — Same as create: target parent must be a root category.
4. **Name-parent conflict** — A child category cannot share the exact same name as its parent.
5. **Duplicate name prevention** — Name uniqueness within the merchant (via `CategoryByNameSpec`, excluding current category).
6. **Smart display order recalculation** — Only triggered when the category is moving to a new parent group or when `DisplayOrder` is explicitly changed.

#### DeleteCategory

**Command:** `src/QRDine.Application/Features/Catalog/Categories/Commands/DeleteCategory/DeleteCategoryCommand.cs`  
**Handler:** `src/QRDine.Application/Features/Catalog/Categories/Commands/DeleteCategory/DeleteCategoryCommandHandler.cs`

**Endpoint:** `DELETE /api/v1/management/categories/{id}`  
**Auth:** `Merchant` role required  
**Output:** `204 No Content`

**Business rules:**
1. **Ownership check** — The category must belong to the current merchant.
2. **No deletion with sub-categories** — If the category has children, deletion is blocked (`CategoryHasChildrenSpec`).
3. **No deletion with products** — If the category has associated products, deletion is blocked (`CategoryHasProductsSpec`).

#### GetMyCategories

**Query:** `src/QRDine.Application/Features/Catalog/Categories/Queries/GetMyCategories/GetMyCategoriesQuery.cs`  
**Handler:** `src/QRDine.Application/Features/Catalog/Categories/Queries/GetMyCategories/GetMyCategoriesQueryHandler.cs`

**Endpoint:** `GET /api/v1/management/categories`  
**Auth:** `Merchant` role required  
**Output:** `List<CategoryTreeDto>` — Flat list built into a tree structure

Retrieves all categories belonging to the current merchant's `MerchantId` (from JWT), maps them to `CategoryTreeDto`, and builds a hierarchical tree using `CategoryTreeExtensions.BuildTree()`.

#### GetCategoriesByMerchant (Storefront)

**Query:** `src/QRDine.Application/Features/Catalog/Categories/Queries/GetCategoriesByMerchant/GetCategoriesByMerchantQuery.cs`  
**Handler:** `src/QRDine.Application/Features/Catalog/Categories/Queries/GetCategoriesByMerchant/GetCategoriesByMerchantQueryHandler.cs`

**Endpoint:** `GET /api/v1/storefront/merchants/{merchantId}/categories`  
**Auth:** Public (no authentication required)  
**Output:** `List<CategoryTreeDto>`

Retrieves all categories for a specific merchant by `merchantId` route parameter. Returns the same tree structure as `GetMyCategories`.

### Products

#### CreateProduct

**Command:** `src/QRDine.Application/Features/Catalog/Products/Commands/CreateProduct/CreateProductCommand.cs`  
**Handler:** `src/QRDine.Application/Features/Catalog/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`

**Endpoint:** `POST /api/v1/management/products`  
**Auth:** `Merchant` role required  
**Content-Type:** `multipart/form-data`  
**Input:** `CreateProductForm` → `CreateProductDto`  
**Output:** `ProductResponseDto`

**Business rules:**
1. **Products must be in sub-categories** — The target category must have a `ParentId` (it must be a child category). Root categories cannot contain products.
2. **Duplicate name prevention** — Product names must be unique within the same category (`ProductNameConflictSpec`).
3. **Optional image upload** — If `ImageFile` is provided, it is uploaded to Cloudinary via `IFileUploadService`.

---

## Specifications

All specifications extend `Specification<T>` from the Ardalis.Specification library and are used with `IRepository.AnyAsync()` or `IRepository.ListAsync()`.

| Specification | File | Purpose |
|--------------|------|---------|
| `CategoriesByMerchantSpec` | `src/QRDine.Application/Features/Catalog/Categories/Specifications/CategoriesByMerchantSpec.cs` | Filters categories by `MerchantId`, ordered by `DisplayOrder` |
| `CategoryByNameSpec` | `src/QRDine.Application/Features/Catalog/Categories/Specifications/CategoryByNameSpec.cs` | Checks name uniqueness within a merchant (with optional `excludeId`) |
| `CategoryHasChildrenSpec` | `src/QRDine.Application/Features/Catalog/Categories/Specifications/CategoryHasChildrenSpec.cs` | Checks if a category has sub-categories |
| `CategoryHasProductsSpec` | `src/QRDine.Application/Features/Catalog/Categories/Specifications/CategoryHasProductsSpec.cs` | Checks if a category has any associated products |
| `CategoryNameConflictSpec` | `src/QRDine.Application/Features/Catalog/Categories/Specifications/CategoryNameConflictSpec.cs` | Checks name+parent uniqueness (for update operations) |
| `ProductNameConflictSpec` | `src/QRDine.Application/Features/Catalog/Products/Specifications/ProductNameConflictSpec.cs` | Checks product name uniqueness within a category |

---

## Category Tree Builder

**File:** `src/QRDine.Application/Features/Catalog/Categories/Extensions/CategoryTreeExtensions.cs`

The `BuildTree()` extension method converts a flat list of `CategoryTreeDto` into a nested tree structure:

```csharp
public static List<CategoryTreeDto> BuildTree(this IEnumerable<CategoryTreeDto> flatCategories)
{
    var categoryList = flatCategories.ToList();

    var childrenLookup = categoryList
        .Where(c => c.ParentId != null)
        .ToLookup(c => c.ParentId);

    var rootCategories = categoryList
        .Where(c => c.ParentId == null)
        .ToList();

    foreach (var root in rootCategories)
    {
        root.Children = childrenLookup[root.Id].ToList();
    }

    return rootCategories;
}
```

---

## Custom Repository Methods

**Interface:** `src/QRDine.Application/Features/Catalog/Repositories/ICategoryRepository.cs`  
**Implementation:** `src/QRDine.Infrastructure/Catalog/Repositories/CategoryRepository.cs`

| Method | Purpose |
|--------|---------|
| `ShiftDisplayOrdersAsync(parentId, fromOrder)` | Increments `DisplayOrder` by 1 for all sibling categories at or above the specified position |
| `GetMaxDisplayOrderAsync(parentId)` | Returns the highest `DisplayOrder` value among siblings |

Both methods use EF Core bulk operations (`ExecuteUpdateAsync`, `MaxAsync`) for performance.

---

## Endpoint Summary

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/api/v1/management/categories` | Merchant | Create category |
| `GET` | `/api/v1/management/categories` | Merchant | Get own categories (tree) |
| `PUT` | `/api/v1/management/categories/{id}` | Merchant | Update category |
| `DELETE` | `/api/v1/management/categories/{id}` | Merchant | Delete category |
| `POST` | `/api/v1/management/products` | Merchant | Create product (form-data) |
| `GET` | `/api/v1/storefront/merchants/{merchantId}/categories` | Public | Get merchant categories (tree) |
