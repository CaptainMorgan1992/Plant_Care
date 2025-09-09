# Plant Care Blazor App

A modern web application for managing and caring for your household plants. Built with [Blazor Server](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) and [MudBlazor](https://mudblazor.com/) components, with Auth0 authentication and secure backend validation.

---

## 🌱 Features

- **User Authentication** via [Auth0](https://auth0.com/)
- **Role-based access:** Only admin users can create new plants
- **Search plants:** Find plants by name (case-insensitive, secure input)
- **Add new plants:** (Admin only) Specify name, description, image URL, origin, watering frequency
- **Plant household:** Add and remove plants from your household
- **Watering notifications:** Scheduled reminders powered by [Quartz.NET](https://www.quartz-scheduler.net/)
- **Secure input validation:** Protection against XSS, SQL injection, and malicious URLs
- **Responsive UI:** Built with MudBlazor for a clean, mobile-friendly interface

---

## 🛠 Tech Stack

- **.NET 9.0** (Blazor Server)
- **MudBlazor** (UI components)
- **Auth0.AspNetCore.Authentication** (user login, roles)
- **Entity Framework Core** (SQLite by default)
- **Quartz.NET** (background jobs)
- **Logging:** Built-in .NET logging

---

## ⚡️ Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A configured [Auth0](https://auth0.com/) tenant (Domain and ClientId)
- [SQLite](https://www.sqlite.org/) (default) or SQL Server

### Installation

1. **Clone the repository**

    ```bash
    git clone https://github.com/CaptainMorgan1992/Plant_Care.git
    cd Plant_Care
    ```

2. **Configure Auth0**

    - Copy `appsettings.json.example` to `appsettings.json`
    - Add your Auth0 `Domain` and `ClientId`:
      ```json
      {
        "Auth0": {
          "Domain": "YOUR_AUTH0_DOMAIN",
          "ClientId": "YOUR_AUTH0_CLIENT_ID"
        },
        "ConnectionStrings": {
          "DefaultConnection": "Data Source=plantcare.db"
        }
      }
      ```
    - **Do not commit your secrets to Github!**

3. **Restore dependencies**

    ```bash
    dotnet restore
    ```

4. **Apply database migrations**

    ```bash
    dotnet ef database update
    ```

5. **Run the application**

    ```bash
    dotnet run
    ```

6. **Browse to**

    ```
    http://localhost:3000
    ```

---

## 🔒 Configuration & Environment Variables

- **Auth0:**  
  Set your Auth0 Domain and ClientId in `appsettings.json` or via environment variables.
    - `Auth0:Domain`
    - `Auth0:ClientId`
- **Database:**  
  By default, uses SQLite with the file `plantcare.db`.
  You can switch to SQL Server by updating the connection string.

---

## 📦 Main Dependencies

```text
Auth0.AspNetCore.Authentication
MudBlazor
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.SqlServer
Quartz
HtmlSanitizer
```

---

## 🔔 Scheduled Jobs

Plant watering reminders are scheduled using Quartz.NET, based on the `WaterFrequency` enum:

| Frequency | Interval      |
|-----------|--------------|
| Low       | 1 minute     |
| Normal    | 30 seconds   |
| High      | 15 seconds   |

Jobs are auto-configured at startup.

---

## 🧩 Project Structure

```text
/Auth0_Blazor
  /Data           - EF Core DB context
  /Enums          - WaterFrequency enum, etc.
  /Jobs           - Quartz job(s) for notifications
  /Models         - Plant and related models
  /Services       - PlantService, UserService, etc.
  /Pages          - Blazor pages (e.g. AddNewPlant.razor)
  /wwwroot        - Static assets
  appsettings.json
  Program.cs      - Startup configuration
```

---

## 🛡 Security

- All input is validated both client- and server-side against XSS, SQL injection, and malformed URLs.
- Only authenticated users can access plant data.
- Only admins can add new plants (checked both in UI and backend).
- Sensitive configuration values (Auth0 keys, DB connection) should **never be committed**—use environment variables or user secrets.

---

## 🚀 Contributing

1. Fork the repo and create your branch (`git checkout -b feature/foo`)
2. Commit your changes
3. Push to the branch (`git push origin feature/foo`)
4. Create a Pull Request

Please follow .NET and MudBlazor coding standards.

---

## 📄 License

[MIT](LICENSE)

---

## 📷 Screenshots

_Add screenshots or demo GIFs here!_

---

## ❓ FAQ / Troubleshooting

- **Auth0 login issues:** Double-check your Domain/ClientId in `appsettings.json` and Auth0 dashboard.
- **Database errors:** Check your connection string and run migrations.
- **Quartz jobs not running:** Ensure Quartz is configured at startup (see `Program.cs`).

---

## 📬 Contact

Made by [CaptainMorgan1992](https://github.com/CaptainMorgan1992).  
For issues or feature requests, use [Github Issues](https://github.com/CaptainMorgan1992/Plant_Care/issues).

