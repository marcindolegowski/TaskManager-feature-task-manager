# TaskManager

TaskManager is a .NET 8 project for managing tasks with a SQL Server database and RabbitMQ integration.

## Features
- **Database migrations**: Automated database migrations using DbUp.
- **API documentation**: Swagger integration for easy API exploration.
- **Message bus**: RabbitMQ integration for event-driven communication.

## Requirements
Before you begin, ensure you have the following installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/)
- **SQL Server** (via Docker or local installation)
- **RabbitMQ** (via Docker or local installation)

## Installation

Follow these steps to set up and run the project:


### 1. Start required services
Use Docker Compose to start SQL Server and RabbitMQ:
```bash
cd items
docker-compose up -d
```

### 2. Initialize the database
Run the following command to initialize the database:
```bash
sqlcmd -S localhost,1433 -U SA -P "P@ssw0rd123!" -d master -i "./sqlserver/db-init.sql"
```

### 3. Run TaskManager.DatabaseMigrations project. 
Project applies necessary migrations to your database.


### 4. Run TaskManager.Api
Once the project is running, you can access the API documentation using swagger.

