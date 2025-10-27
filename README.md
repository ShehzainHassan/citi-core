# citi-core
A lightweight, high-performance .NET 9 backend built for powering mobile banking platform.

## Overview
Citi-core is a robust, scalable backend solution designed to manage banking infrastructure . Built with .NET 9.0, it provides a RESTful API that can be integrated with any frontend application

## Features
- **Account Management** Complete CRUD operations for account management
- **Card Operations** Card issuance, blocking/unblocking, and card limits handling
- **Transaction Processing**  Secure money transfers, real-time balance updates, and transaction history tracking
- **RESTful API**: Clean, well-documented endpoints following REST best practices

## Technology Stack
- **.NET 9.0**: Latest .NET framework for optimal performance
- **Entity Framework Core 9.0**: ORM for database operations
- **Microsoft SQL Server**: Database option for production environments
- **Swagger/OpenAPI**: API documentation and testing

## Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Installation
1. Clone the repository

   ```bash
   git clone https://github.com/ShehzainHassan/citi-core
   cd citi-core
   ```

2. Restore dependencies

   ```bash
   dotnet restore
   ```

3. Build the project
   ```bash
   dotnet build
   ```
## Development

### Project Structure

- `Controllers/`: API endpoints
- `Models/`: Data models
- `Data/`: Repository pattern implementation and database context
- `Properties/`: Launch settings

### Adding Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```
## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
