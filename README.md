# 🛒 MiniECommerce

A full-featured e-commerce web application built with **ASP.NET Core MVC**, **C#**, and **Entity Framework Core**.

The project focuses on maintainable code, separation of concerns, secure request handling, and practical application of software design patterns.

## ✨ Features

### 🛍️ Shopping Experience

* Browse products and explore categories.
* View product details and available stock.
* Add products to a session-based shopping cart.
* Update cart quantities asynchronously using AJAX.
* Calculate cart totals and manage cart items.

### 👤 Authentication & Authorization

* User registration and login using ASP.NET Core Identity.
* Role-based authorization for Admin and Customer roles.
* Protected administrative functionality.

### ⚙️ Admin Dashboard

* Manage products and categories.
* View customer orders and order details.
* Track and manage order statuses.
* Support product image management.

### 🔒 Security

* Anti-forgery protection for state-changing requests.
* Anti-forgery token validation for AJAX requests.
* Server-side validation.
* Authentication and authorization for protected resources.

## 🧰 Tech Stack

| Technology            | Purpose                                 |
| --------------------- | --------------------------------------- |
| C#                    | Application development                 |
| ASP.NET Core MVC      | Web application framework               |
| Entity Framework Core | Object-relational mapping               |
| Microsoft SQL Server  | Relational database                     |
| ASP.NET Core Identity | Authentication and authorization        |
| Razor Views           | Server-side UI rendering                |
| HTML, CSS, JavaScript | Frontend development                    |
| Bootstrap and jQuery  | UI styling and client-side interactions |
| AJAX                  | Asynchronous requests                   |
| Git and GitHub        | Version control and source management   |

## 🏗️ Architecture & Design Patterns

The application separates presentation logic, business logic, and data-access responsibilities to improve maintainability and readability.

* **MVC Pattern:** Organizes the application into Models, Views, and Controllers.
* **Repository Pattern:** Encapsulates data-access operations.
* **Generic Repository:** Provides reusable data-access functionality.
* **Unit of Work Pattern:** Groups repository operations behind a common abstraction.
* **Service Layer:** Keeps business logic separate from controllers.
* **Dependency Injection:** Provides dependencies through ASP.NET Core's built-in DI container.
* **ViewModels:** Shape data specifically for presentation requirements.
* **Entity Framework Core Code-First:** Manages database schema changes through migrations.

The project applies separation of concerns and Clean Code principles within a single ASP.NET Core MVC application.

## 🗄️ Database

The application uses **Microsoft SQL Server** with **Entity Framework Core**.

The data model includes entities such as:

* `ApplicationUser`
* `Product`
* `Category`
* `Order`
* `OrderItem`

Database relationships and schema changes are managed through the application's DbContext configuration and EF Core migrations.

## 📁 Project Structure

```text
MiniECommerce/
├── Areas/
├── Controllers/
├── Data/
├── Extensions/
├── Migrations/
├── Models/
├── Repository/
├── S
```
