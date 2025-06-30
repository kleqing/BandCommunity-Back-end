# Band Community Back-end

Back-end version of Band Community

For front-end version, please visit [BandCommunity-Front-end](https://github.com/kleqing/BandCommunity-Front-end)

## Installation

Install [.NET](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) SDK (8.0)

```csharp
# Install dotnet ef core (If already installed, skip this section)
dotnet tool install --global dotnet-ef
```

## Usage

```bash
# Clone the repository
git clone https://github.com/kleqing/BandCommunity-Back-end
```

Before run the application, make sure you have config the `.env` file first. The file is locate at `BandCommunity.WebApi/.env.template`

```csharp
# Restore project
dotnet restore

# Build project
dotnet build

# Create database from model (required dotnet ef core)
dotnet ef migrations add "Initial" --project BandCommunity.Infrastructure  --startup-project BandCommunity.WebApi --context ApplicationDbContext
dotnet ef database update --project BandCommunity.Infrastructure  --startup-project BandCommunity.WebApi --context ApplicationDbContext 

// API Endpoint: https://localhost:7160/swagger/index.html
```

## Project Structure
```
├── Application
│   ├── Common // Response data
│   └── Services // Services for the application
│       ├── Auth // Authorization
│       ├── Email // Email sender
│       └── Role // Seed role (For .NET Identity)
├── Domain
│   ├── DTO // Request data
│   │   └── Auth 
│   ├── Entities // Models for the system
│   ├── Enums 
│   ├── Interfaces // Inheritors from  Repository
│   └── JWT // Json Web Token
├── Infrastructure
│   ├── Auth // Interfaces of Application
│   ├── Data // DbContext
│   └── Migrations // Migrate from context
├── Repository
│   └── Repositories
├── Shared
│   ├── Constant
│   ├── Exceptions // Throw exception messages
│   ├── Helper 
│   └── Utility
└── WebApi
    ├── Controller
    ├── .env // Configuration
    └── Program.cs

```

## Environment

- PostgreSQL 17
- Jetbrains Rider (Other IDE still work)
- TablePlus (SQL Management)

**NOTE: IF YOU PREFER TO USE OTHER SQL THAN POSTGRE, PLEASE REMOVE POSTGRESQL PACKAGE IN THE PROJECT AND USE YOUR OWN!**

