I have completed the full project screen recording as requested.
Please find the Google Drive link below:
🔗 Project Screen Recording: [https://drive.google.com/file/d/1sL9NzIZwrjTmE9_DDctvIsu8KQhZm3DT/view?usp=drive_link]
If you need anything else, please let me know.

# Gadget BD – ASP.NET Core MVC E-Commerce + Role Management System

A fully functional **ASP.NET Core MVC 8** application with complete **Product Management**, **Shopping Cart**, **Orders**, **Identity Authentication**, and **Role-based Authorization**.

---
## ⭐ Project Summary
Gadget BD একটি complete e-commerce style system যেখানে আছে—
- 🛒 Shopping Cart  
- 📦 Product CRUD  
- 📝 Product Logs (Created / Updated / Deleted history)  
- 📑 Order & Checkout System  
- 🔐 Identity + Roles (ADMIN, MANAGER, USER)  
- 🔍 Search, Sorting & Pagination  
- 🎨 Responsive UI (Bootstrap 5 + Icons)  
- 🗃 Session-based Cart Storage  
---

## 🛠️ Technologies Used
| Area | Technology |
|------|------------|
| Backend | ASP.NET Core MVC 8 |
| Authentication | Identity Core + Roles |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Frontend | Bootstrap 5, jQuery |
| Cart Storage | Session + JSON |
| Logging | Custom Product Logs Table |
---

## 🧩 Business Features

### ✔ Product Module
- Add / Edit / Delete Product  
- Upload Product Image  
- Pagination + Search + Sorting  
- Product Log (CRUD history with user & timestamp)  
- ADMIN & MANAGER access only  

### ✔ Shopping Cart Module
- Session-based cart  
- Add multiple items  
- Auto quantity update  
- Navbar cart badge  
- Remove item & clear cart  

### ✔ Order Module
- Checkout with delivery info  
- Saves Order + OrderItems  
- User can view **My Orders**  
- Admin/Manager can view all orders  
- Order status management  

### ✔ Role Management
Admin can—
- Create roles  
- Assign roles  
- View user list  
- Manage User roles  

### ✔ Identity User Extension
Extra fields added:

```csharp
public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public string CellPhone { get; set; }
    public string Country { get; set; }
}
```
---

## 📁 Project Folder Structure

```
📂 AuthTest_RoleBased
│── Controllers/
│   ├── HomeController.cs
│   ├── ProductsController.cs
│   ├── ShoppingController.cs
│   ├── RoleController.cs
│
│── Data/
│   ├── ApplicationDbContext.cs
│   ├── ApplicationUser.cs
│
│── Models/
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── ProductLog.cs
│   ├── PaginatedList.cs
│   ├── ErrorViewModel.cs
│
│── Models/ViewModels/
│   ├── EditUserViewModel.cs
│   ├── ShowCartViewModel.cs
│
│── Views/
│   ├── Products/
│   ├── Shopping/
│   ├── Role/
│   ├── Shared/
│
│── wwwroot/
│   ├── css/
│   ├── js/
│   ├── images/
│
│── SessionExtension.cs
│── Program.cs
│── appsettings.json
```
---

## 🗃 Database ER Diagram (Simple Text Form)

```
ApplicationUser (1) ---------- (∞) Orders
        |
        ∞
 ProductLogs

Products (1) ---------- (∞) OrderItems ---------- (∞) Orders

Products (1) ---------- (∞) ProductLogs
(Only ProductId saved as value — no FK)
```

### Tables Overview

| Table | Purpose |
|-------|---------|
| `Products` | Product list |
| `Orders` | User orders |
| `OrderItems` | Each product inside an order |
| `ProductLogs` | Track product CRUD actions |
| `AspNetUsers` | Identity users |
| `AspNetRoles` | Role-based access |

---

## 🏗 MVC Architecture Flow

```
Browser ──► Controller ──► Model ──► DB  
   ▲             │  
   └─────────────┴── View (HTML Render)
```
---

## 🔁 Controller → View Workflow (Shopping)

```
ShoppingController
    ├── Index() → All Products
    ├── AddToCart(id) → Save in Session
    ├── ShowCart() → Display Cart
    ├── Checkout() → User Info + Cart
    └── PlaceOrder() → Save Order + Clear Cart
```
---

## 🧾 Session Extension (Used for Cart)

```csharp
public static void SetObject<T>(this ISession session, string key, T value)
{
    session.SetString(key, JsonConvert.SerializeObject(value));
}

public static T GetObject<T>(this ISession session, string key)
{
    var value = session.GetString(key);
    return value == null ? default : JsonConvert.DeserializeObject<T>(value);
}
```
---

## 📑 Recommended Commit Messages

| Type | Example Message |
|------|-----------------|
| ✨ Feature | `feat: add product create/edit/delete module` |
| 📦 Update | `update: add cart session functionality` |
| 🔧 Fix | `fix: pagination bug on product list` |
| 🎨 UI | `style: redesigned navbar with role-based color theme` |
| 📝 Docs | `docs: added ER diagram & feature list to README` |
| 🔐 Security | `security: restrict product logs to admin only` |

---

## 📊 Feature Permission Matrix

| Feature | User | Manager | Admin |
|--------|------|---------|--------|
| Shopping | ✔ | ✔ | ✔ |
| Checkout | ✔ | ✔ | ✔ |
| Product CRUD | ✖ | ✔ | ✔ |
| View Logs | ✖ | ✖ | ✔ |
| Role Assign | ✖ | ✖ | ✔ |
| Order List | ✖ | ✔ | ✔ |

---
## 👨‍💻 Developer

**Md Tofayel Ahmed**  
📧 Email: **tofayelkhan555@gmail.com**  
🌐 Facebook: **https://facebook.com/tofayel555**

---Thank you---

## 📄 License
This project is for learning & personal development purposes.



Thank you.
