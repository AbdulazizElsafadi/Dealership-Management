# Dealership Management System

A .NET 9.0 Web API for managing car dealership operations including vehicle inventory, customer management, and sales tracking.

## Project Structure

```
Dealership Management/
├── Controllers/           # API Controllers
│   └── VehiclesController.cs
├── Data/                  # Data Access Layer
│   └── DealershipDbContext.cs
├── DTOs/                  # Data Transfer Objects
│   ├── VehicleDto.cs
│   ├── CustomerDto.cs
│   └── SaleDto.cs
├── Models/                # Entity Models
│   ├── Vehicle.cs
│   ├── Customer.cs
│   └── Sale.cs
├── Services/              # Business Logic Layer
│   ├── IVehicleService.cs
│   └── VehicleService.cs
├── Configurations/        # Configuration Classes
├── Middleware/           # Custom Middleware
├── Extensions/           # Extension Methods
├── Program.cs            # Application Entry Point
├── appsettings.json      # Configuration
└── README.md            # This File
```

## Features

### Core Entities

- **Vehicle**: Car inventory management with details like make, model, year, VIN, price, etc.
- **Customer**: Customer information management
- **Sale**: Sales transaction tracking

### API Endpoints

#### Vehicles

- `GET /api/vehicles` - Get all vehicles
- `GET /api/vehicles/{id}` - Get vehicle by ID
- `GET /api/vehicles/available` - Get available vehicles only
- `GET /api/vehicles/search` - Search vehicles with filters
- `POST /api/vehicles` - Create new vehicle
- `PUT /api/vehicles/{id}` - Update vehicle
- `DELETE /api/vehicles/{id}` - Delete vehicle

## Technology Stack

- **.NET 9.0**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server LocalDB**
- **Swagger/OpenAPI**

## Setup Instructions

### Prerequisites

- .NET 9.0 SDK
- SQL Server LocalDB (included with Visual Studio)

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd "Dealership Management"
   ```

2. **Install dependencies**

   ```bash
   dotnet restore
   ```

3. **Create database**

   ```bash
   dotnet ef database update
   ```

4. **Run the application**

   ```bash
   dotnet run
   ```

5. **Access the API**
   - API: `https://localhost:7220`
   - Swagger UI: `https://localhost:7220/swagger`

## Database Schema

### Vehicle Table

- Id (Primary Key)
- Make (Required)
- Model (Required)
- Year (Required)
- VIN (Unique)
- Price (Required)
- Color
- Transmission
- FuelType
- Mileage
- Description
- IsAvailable
- CreatedAt
- UpdatedAt
- SaleId (Foreign Key)

### Customer Table

- Id (Primary Key)
- FirstName (Required)
- LastName (Required)
- Email (Required, Unique)
- Phone
- Address
- City
- State
- ZipCode
- CreatedAt
- UpdatedAt

### Sale Table

- Id (Primary Key)
- CustomerId (Foreign Key)
- VehicleId (Foreign Key)
- SalePrice (Required)
- SaleDate (Required)
- PaymentMethod
- Notes
- CreatedAt
- UpdatedAt

## API Documentation

The API documentation is available through Swagger UI at `/swagger` when running in development mode.

## Development

### Adding New Features

1. Create model in `Models/` folder
2. Create DTOs in `DTOs/` folder
3. Add service interface and implementation in `Services/` folder
4. Create controller in `Controllers/` folder
5. Update `Program.cs` to register new services
6. Add database migration if needed

### Database Migrations

```bash
# Create migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update
```

## Testing

The project includes sample data seeded in the database for testing purposes.

## Contributing

1. Follow the existing code structure
2. Add appropriate validation attributes to DTOs
3. Include XML documentation for API endpoints
4. Test all endpoints before submitting

## License

This project is for educational purposes as part of a .NET coding challenge.
