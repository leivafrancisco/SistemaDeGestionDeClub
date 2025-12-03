# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Sistema de Gestión de Club de Fútbol - A comprehensive club management system built with:
- **Backend**: ASP.NET Core 8 Web API with Clean Architecture
- **Database**: SQL Server 2022 with Entity Framework Core 8
- **Authentication**: JWT Bearer tokens

**Note**: This project is currently backend-only. The frontend was removed in a recent migration to cloud database.

## Development Commands

### Backend (ASP.NET Core)

```bash
# Navigate to backend API
cd Backend/API

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the API (starts on https://localhost:5000)
dotnet run

# Run with watch mode (auto-reload on changes)
dotnet watch run
```

### Database Setup

1. Create the database:
```sql
CREATE DATABASE gestion_socios;
```

2. Run the initialization script:
```bash
# Execute gestion_socios_db.sql in SQL Server Management Studio
# This creates all tables and seed data
```

3. Update connection string in `Backend/API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=gestion_socios;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

## Architecture

### Backend Structure (Clean Architecture)

The backend follows Clean Architecture with four distinct layers:

**Domain Layer** (`Backend/Domain/`)
- Pure business entities with no external dependencies
- Contains all domain models: `Persona`, `Usuario`, `Socio`, `Rol`, `Actividad`, `Membresia`, `MembresiaActividad`, `Pago`, `MetodoPago`, `Asistencia`
- Entity relationships are established through navigation properties
- Entities are POCOs with computed properties (e.g., `Membresia.TotalCargado`, `Membresia.Saldo`)

**Application Layer** (`Backend/Application/`)
- `DTOs/`: Data Transfer Objects for API communication
- `Services/`: Business logic interfaces and implementations
  - `ISocioService`, `IAuthService`, `IUsuarioService`, `IActividadService`
  - `IMembresiaService`, `IPagoService`, `IAsistenciaService`
- Contains service interfaces that orchestrate domain logic
- Manual mapping between entities and DTOs

**Infrastructure Layer** (`Backend/Infrastructure/`)
- `Data/ClubDbContext.cs`: EF Core database context with complete entity configurations
- Maps domain entities to database tables using Fluent API
- Implements soft delete pattern via `FechaEliminacion`
- Auto-updates `FechaActualizacion` on entity modification (see ClubDbContext.SaveChangesAsync)

**API Layer** (`Backend/API/`)
- `Controllers/`: REST API endpoints
  - `AuthController`, `SociosController`, `UsuariosController`
  - `ActividadesController`, `MembresiasController`, `PagosController`, `AsistenciasController`
- `Program.cs`: Application configuration including:
  - JWT Bearer authentication (Program.cs:28-41)
  - CORS policy for frontend (Program.cs:53-62)
  - Swagger/OpenAPI with JWT support (Program.cs:65-99)
  - Service registration with DI container (Program.cs:18-24)
  - Health check endpoint at `/health`

### Database Naming Convention

**IMPORTANT**: The database uses snake_case for all column names, while C# entities use PascalCase. EF Core mappings handle this translation:
- Entity Property: `FechaCreacion` → Database Column: `fecha_creacion`
- Entity Property: `NombreUsuario` → Database Column: `nombre_usuario`
- Entity Property: `IdPersona` → Database Column: `id_persona`

All table names are lowercase plural (e.g., `personas`, `usuarios`, `socios`, `membresias`).

### Key Domain Relationships

- `Persona` → One-to-One → `Usuario` (users for authentication)
- `Persona` → One-to-One → `Socio` (club members)
- `Usuario` → Many-to-One → `Rol` (user roles: superadmin, admin, recepcionista)
- `Socio` → One-to-Many → `Membresia` (memberships with flexible date ranges)
- `Membresia` → Many-to-Many → `Actividad` via `MembresiaActividad` (membership activities with price snapshot)
- `Membresia` → One-to-Many → `Pago` (payments for memberships)
- `Pago` → Many-to-One → `MetodoPago` (payment method: efectivo, transferencia, etc.)
- `Pago` → Many-to-One → `Usuario` (user who registered the payment)
- `Socio` → One-to-Many → `Asistencia` (attendance tracking)

### Membership System (Date-Based)

**IMPORTANT**: The system migrated from period-based (año/mes) to flexible date-based memberships.

**Membership Fields**:
- `FechaInicio` (fecha_inicio): Start date of membership
- `FechaFin` (fecha_fin): End date of membership
- ❌ REMOVED: `PeriodoAnio`, `PeriodoMes` (old period-based fields)

**Business Rules**:
- `FechaFin` must be greater than `FechaInicio`
- Cannot have overlapping date ranges for the same socio
- Memberships can span any date range (not limited to monthly periods)
- Validation prevents duplicate/overlapping memberships

**Computed Properties**:
- `TotalCargado`: Sum of all activity prices (from membresia_actividades)
- `TotalPagado`: Sum of all payments for this membership
- `Saldo`: TotalCargado - TotalPagado (outstanding balance)

### Payment System

**Payment Flow**:
1. Admin selects a socio
2. System shows unpaid memberships (saldo > 0)
3. Admin selects payment method from `metodos_pago` table
4. Payment is registered against specific membership
5. System generates receipt (ComprobantePagoDto) with all details
6. Membership saldo is automatically updated

**Payment Methods** (`metodos_pago` table):
- Stored in database, not hardcoded
- Common methods: Efectivo, Transferencia, Tarjeta, etc.
- Can be managed through database

**Payment Receipt Format**:
- Number: `PAG-{id:6 digits}-{year}` (e.g., PAG-000123-2025)
- Includes: socio info, membership details (date range), payment method, amount, saldo before/after

**Key Calculations**:
- `Membresia.TotalCargado` = Sum of all activity prices at the time of membership creation
- `Membresia.TotalPagado` = Sum of all payments for that membership
- `Membresia.Saldo` = TotalCargado - TotalPagado
- Membership is considered paid when Saldo <= 0

### Authentication & Authorization

**JWT Configuration**:
- JWT settings in `appsettings.json` (Jwt:Key, Jwt:Issuer, Jwt:Audience)
- Token includes user claims (user ID, username, email, role)
- API endpoints protected via `[Authorize]` attribute with role requirements
- Token expiration: 24 hours

**Roles**:
- `superadmin`: Full system access, user management, can create admin users
- `admin`: Member management, payments, reports, attendance, can create recepcionista users
- `recepcionista`: Read-only member access, attendance registration, can assign activities

**Password Security**:
- ⚠️ Currently using plaintext passwords (DEVELOPMENT ONLY)
- TODO: Implement BCrypt.Net for production

### API Endpoints

All endpoints follow RESTful conventions. API is accessible at `https://localhost:5000` with Swagger UI at root (`/`).

**Authentication** (`/api/auth`)
- `POST /api/auth/login` - Login with username/password, returns JWT token
- `GET /api/auth/me` - Get current authenticated user info (requires JWT)

**Socios (Members)** (`/api/socios`)
- `GET /api/socios` - List all members (pagination, filtering, search)
  - Query params: `search`, `estaActivo`, `page`, `pageSize`
  - Search across: nombre, apellido, email, dni, numero_socio
- `GET /api/socios/{id}` - Get member by ID
- `GET /api/socios/numero/{numeroSocio}` - Get member by member number
- `POST /api/socios` - Create member (admin only) - Auto-generates numeroSocio format SOC-0001
- `PUT /api/socios/{id}` - Update member (admin only)
- `PUT /api/socios/{id}/desactivar` - Soft delete member (admin only)
- `GET /api/socios/estadisticas/total` - Get total member count

**Actividades (Activities)** (`/api/actividades`)
- `GET /api/actividades` - List all activities (only active, filtered by FechaEliminacion)
- `GET /api/actividades/{id}` - Get activity by ID
- `POST /api/actividades` - Create activity (superadmin/admin only)
- `PUT /api/actividades/{id}` - Update activity (superadmin/admin only)
- `DELETE /api/actividades/{id}` - Soft delete activity (superadmin/admin only)

**Membresías (Memberships)** (`/api/membresias`)
- `GET /api/membresias` - List memberships with filters
  - Query params: `idSocio`, `fechaDesde`, `fechaHasta`, `soloImpagas`, `page`, `pageSize`
- `GET /api/membresias/{id}` - Get membership by ID
- `POST /api/membresias` - Create membership (admin only)
- `PUT /api/membresias/{id}` - Update membership activities (admin only)
- `DELETE /api/membresias/{id}` - Soft delete membership (admin only)
- `GET /api/membresias/estadisticas/total` - Get total membership count
- `POST /api/membresias/asignar-actividad` - Assign activity to membership (recepcionista can do this)
- `POST /api/membresias/remover-actividad` - Remove activity from membership (only if no payments)

**Pagos (Payments)** (`/api/pagos`)
- `GET /api/pagos` - List payments with filters
  - Query params: `idMembresia`, `idSocio`, `idMetodoPago`, `fechaDesde`, `fechaHasta`, `page`, `pageSize`
- `GET /api/pagos/{id}` - Get payment by ID
- `POST /api/pagos` - Register payment and generate receipt (admin only)
- `GET /api/pagos/{id}/comprobante` - Generate payment receipt
- `DELETE /api/pagos/{id}` - Void payment (admin only)
- `GET /api/pagos/metodos` - Get available payment methods from metodos_pago table
- `GET /api/pagos/estadisticas` - Get payment statistics (admin only)
- `GET /api/pagos/estadisticas/recaudacion` - Get total collection by date range (admin only)

**Usuarios (Users)** (`/api/usuarios`)
- `GET /api/usuarios` - List all users (superadmin/admin only)
  - Query params: `rol`, `estaActivo`
- `GET /api/usuarios/{id}` - Get user by ID (superadmin/admin only)
- `POST /api/usuarios` - Create user (superadmin/admin only, with role restrictions)
- `PUT /api/usuarios/{id}` - Update user (superadmin only)
- `PUT /api/usuarios/{id}/desactivar` - Deactivate user (superadmin only)

**Asistencias (Attendance)** (`/api/asistencias`)
- `POST /api/asistencias` - Register attendance (all roles)
- `GET /api/asistencias` - List attendance records with filters
  - Query params: `idSocio`, `fechaDesde`, `fechaHasta`, `page`, `pageSize`

**Health Check**
- `GET /health` - Returns server health status and timestamp

### Soft Delete Pattern

All entities implement soft delete via `FechaEliminacion` (nullable DateTime):
- When set, entity is considered deleted
- Queries should filter by `FechaEliminacion == null` for active records
- `Socio.EstaActivo` provides explicit active/inactive flag
- Entity Framework's `SaveChangesAsync` automatically updates `FechaActualizacion`

### Business Rules

**Membership Management**:
- Cannot create overlapping memberships for same socio (date range validation)
- `FechaFin` must be greater than `FechaInicio`
- Memberships store activity prices at moment of creation (price snapshot in MembresiaActividad)
- Activities can be added/removed from memberships via special endpoints
- Removing activity is only allowed if no payments have been made
- Recepcionistas can assign/remove activities but cannot create/delete memberships

**Activity Management**:
- Activity prices are stored in each MembresiaActividad (historical pricing)
- Changing activity price doesn't affect existing memberships
- Soft delete prevents activity removal if used in active memberships
- ❌ REMOVED: `EsCuotaBase` field - all activities are now treated equally

**Payment Registration**:
- Payments are always associated with a specific membership
- Multiple payments can be made for a single membership (partial payments)
- Payment receipt includes: socio info, membership details, payment method, amount, date
- Only admins and superadmins can register/void payments
- Payment amounts must be positive and cannot exceed membership saldo
- Voiding a payment recalculates the membership saldo

**User Management**:
- Admin can create ONLY recepcionista users
- Superadmin can create admin or recepcionista users
- Username and email must be unique
- User creation validates creator's permissions via `rolCreador` parameter

**Socio Management**:
- Email and DNI must be unique across all personas
- `NumeroSocio` is auto-generated in format SOC-0001, SOC-0002, etc.
- Desactivar sets both `FechaEliminacion` and `FechaBaja`

### Development Notes

- API runs on `https://localhost:5000` with Swagger UI at root (`/`)
- CORS is configured to allow requests from localhost:3000 (for future frontend)
- Database migrations are NOT used - schema is managed via `gestion_socios_db.sql`
- All dates use SQL Server's `GETDATE()` for defaults
- Entity Framework tracks changes and auto-updates `FechaActualizacion` on save
- **Member numbers (numeroSocio)** are auto-generated by backend in SocioService.CrearAsync (format: SOC-0001, SOC-0002, etc.)
- CrearSocioDto does NOT include numeroSocio field - it's generated server-side
- Backend filters inactive entities (FechaEliminacion != null) in service layer

### Important Field Changes & Migrations

**Removed Fields**:
- ❌ `Actividad.EsCuotaBase` / `es_cuota_base` - Removed from entity, DTOs, services, and all components
  - All activities are now treated equally when calculating membership totals
  - No special "base fee" concept exists in the system

**Migrated Fields (2025-11-28)**:
- ❌ `Membresia.PeriodoAnio`, `Membresia.PeriodoMes` (old period-based fields)
- ✅ `Membresia.FechaInicio`, `Membresia.FechaFin` (new date-based fields)
- Migration allows flexible date ranges instead of fixed monthly periods
- Validation changed from "duplicate period" to "overlapping dates"

**Auto-Generated Fields**:
- `Socio.NumeroSocio` - Auto-generated in format SOC-0001, SOC-0002, etc.
  - Generated server-side by SocioService.CrearAsync
  - NOT included in CrearSocioDto
  - Sequential numbering based on max existing number + 1

### Design Patterns Used

**Soft Delete Pattern**:
- All entities have `FechaEliminacion` nullable field
- Enables data recovery and audit trails

**Service Layer Pattern**:
- Business logic encapsulated in services with interfaces
- Dependency injection for all services

**DTO Pattern**:
- Separation between domain entities and data transfer objects
- Manual mapping in service layer

**Fluent API Configuration**:
- Entity relationships configured in ClubDbContext
- Column mappings (PascalCase → snake_case)
- Default values and constraints

**Repository Pattern** (Implicit):
- EF Core DbSet<T> acts as repository
- ClubDbContext acts as Unit of Work

### Test Credentials

- **Admin**: `admin` / `admin123` (role: admin)
- **Recepcionista**: `recepcionista` / `recep123` (role: recepcionista)

### Additional Documentation

For more detailed information, refer to:
- `README-ARQUITECTURA.txt` - Complete architecture and business rules documentation
- `DiagramaClases.md` - UML class diagram (Mermaid format)
- `README.md` - Project setup and installation guide
- `gestion_socios_db.sql` - Database schema and seed data
