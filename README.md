# 🏢 Real Estate Management System

A comprehensive, full-stack web application developed using **ASP.NET Core MVC** and **SQL Server 2022** to streamline property management, buyer inquiries, and real estate listings.

---

## 📋 Course Project Information
* **Course:** .NET TECHNOLOGIES – 01CE15230 (B.Tech Semester 5)
* **Component:** Continuous Semester Evaluation (CSE) Mini Project
* **Technology Stack:** C#, ASP.NET Core MVC, Entity Framework Core, SQL Server 2022

---

## 📸 Application Overview & Key Modules

| Module | Description |
| :--- | :--- |
| 🔐 **Authentication & Authorization** | Secure Login and Registration with Role-Based Access Control (Admin / Customer). |
| 📊 **Admin Dashboard** | Real-time system analytics — Total Properties, Available Properties, Inquiries Count, and Users. |
| 🏡 **Property Management** | Full CRUD operations (Add, Edit, View, Delete) for property listings with image uploads. |
| 🔍 **Property Search & Filter** | Dynamic search by Property Type (Flat, House, Land), Price Range, and Location. |
| 📑 **Inquiry System** | Customers can send direct purchase/rental inquiries to administrators. |

---

## ✨ Features
* ✅ **Clean Architecture:** Built using the MVC (Model-View-Controller) design pattern.
* ✅ **Entity Framework Core ORM:** Code-First approach for managing SQL Server database operations.
* ✅ **Responsive Design:** User-friendly UI built with Bootstrap 5 for seamless desktop and mobile view.
* ✅ **Security:** Input validation, anti-forgery tokens, and password protection.
* ✅ **Free Live Deployment:** Hosted online for real-time remote evaluation and public usage.

---

## 🛠️ Technology Stack

| Category | Technology | Version | Description |
| :--- | :--- | :--- | :--- |
| **Backend Framework** | ASP.NET Core MVC | 8.0 | Web Application Framework & Request Pipeline |
| **Programming Language** | C# | Latest | Core Business Logic & Controller Implementation |
| **Database** | Microsoft SQL Server | 2022 | Relational Database Management System (RDBMS) |
| **ORM Tool** | Entity Framework Core | 8.0 | Database Access & Code-First Migrations |
| **Frontend Framework** | Bootstrap | 5.3 | Responsive UI Layout & Styling |
| **Client Interface** | HTML5 / CSS3 / JavaScript | Standard | Modern Web Interfaces & Dynamic Client Interactivity |
| **Version Control** | Git & GitHub | Latest | Code Management & Collaboration |

---

## 👥 Team Members

| Name | Enrollment No. |
| :--- | :--- |
| **[Maitri Adroja]** | [92400103446] |
| **[Krupa Makwana]** | [92400103408] |
| **[Manthan Makwana]** | [92400103407] |

---

## 🏗️ Project Directory Structure

```text
RealEstateManagementSystem/
│
├── 📁 Controllers/          → Handles incoming HTTP requests and application logic
│   ├── HomeController.cs
│   ├── AccountController.cs
│   └── PropertyController.cs
│
├── 📁 Models/               → Business Entities and Database Models
│   ├── User.cs
│   ├── Property.cs
│   └── Inquiry.cs
│
├── 📁 Views/                → Razor UI Pages
│   ├── Home/
│   ├── Account/
│   ├── Property/
│   └── Shared/
│
├── 📁 Data/                 → Database Context & EF Core Migrations
│   └── ApplicationDbContext.cs
│
├── 📁 wwwroot/              → Static Files (CSS, JS, Uploaded Property Images)
│   ├── css/
│   ├── js/
│   └── images/
│
├── appsettings.json         → App Configuration & Database Connection Strings
└── Program.cs               → Application Pipeline & Service Configurations

💻 Local Setup & Execution Guide
1️⃣ Prerequisites
Make sure you have the following installed on your local machine:

Visual Studio 2022 (with .NET and web development workload selected)

SQL Server 2022 & SSMS

Git

2️⃣ Installation Steps
Clone the Repository:

Bash
git clone [https://github.com/Maitri972/RealEstateManagementSystem.git](https://github.com/Maitri972/RealEstateManagementSystem.git)
cd RealEstateManagementSystem
Open in Visual Studio:

Launch Visual Studio 2022 and open RealEstateManagementSystem.sln.

Configure SQL Server Connection:

Update the connection string in appsettings.json:

JSON
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_LOCAL_SQL_SERVER;Database=RealEstateDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
Apply Database Migrations:

Open Package Manager Console (Tools -> NuGet Package Manager -> Package Manager Console) and run:

Bash
Update-Database
Run the Project:

Press F5 or click IIS Express / Start to launch the site locally.

📄 License
This project is developed for educational purposes under the B.Tech Computer Engineering Curriculum. 

🌐 Live Application URL
🚀 Live Demo: Updated here.



⭐ Star this repository if you find it helpful!
