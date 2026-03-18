# Catalog Module

Menu and table management for restaurants.

## Quick Overview

The Catalog module manages the core menu structure for each merchant: categories, products, topping groups, toppings, and restaurant tables.

## Key Features

- ✅ Hierarchical category organization (2 levels: root → child)
- ✅ Product management with Cloudinary image uploads
- ✅ Table management with QR code generation
- ✅ Customization options via topping groups and toppings
- ✅ Display order management for sorting
- ✅ Real-time table occupancy tracking

## Entities

| Entity           | Purpose                                         |
| ---------------- | ----------------------------------------------- |
| **Category**     | Menu categories (e.g., Appetizers, Main Course) |
| **Product**      | Menu items with pricing and images              |
| **Table**        | Physical restaurant tables with QR codes        |
| **ToppingGroup** | Container for topping options (e.g., Sauces)    |
| **Topping**      | Individual customization options                |

## Use Cases

1. **Owner** creates menu category hierarchy
2. **Owner** adds products with images and pricing
3. **Owner** sets up physical tables with QR codes
4. **Owner** defines customization options (toppings)
5. **Customer** scans table QR code to view menu
6. **Customer** orders products with customizations

## API Endpoints

| Method   | Path                                 | Auth     | Purpose            |
| -------- | ------------------------------------ | -------- | ------------------ |
| `POST`   | `/api/v1/management/categories`      | Merchant | Create category    |
| `GET`    | `/api/v1/management/categories`      | Merchant | Get own categories |
| `PUT`    | `/api/v1/management/categories/{id}` | Merchant | Update category    |
| `DELETE` | `/api/v1/management/categories/{id}` | Merchant | Delete category    |
| `POST`   | `/api/v1/management/products`        | Merchant | Create product     |
| `POST`   | `/api/v1/management/tables`          | Merchant | Create table       |
| `GET`    | `/api/v1/storefront/categories`      | Public   | Browse categories  |
| `GET`    | `/api/v1/storefront/products`        | Public   | Browse products    |

## Documentation

- **[Detailed Module Documentation](catalog-module.md)** — Complete documentation with all entities, CQRS commands/queries, specifications, and endpoints

---

**Reference:** See also [Features Overview](../) for other modules and [Development Guidelines](../../development/) for implementation patterns.
