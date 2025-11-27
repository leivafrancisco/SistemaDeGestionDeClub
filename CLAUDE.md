# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Sistema de Gestión de Club de Fútbol - A comprehensive management system for sports clubs built with:
- **Backend**: ASP.NET Core 8 Web API with Clean Architecture
- **Frontend**: Next.js 14 (App Router) with TypeScript and TailwindCSS
- **Database**: SQL Server 2022 with Entity Framework Core 8

## Development Commands

### Backend (ASP.NET Core)

```bash
# Navigate to backend
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

### Frontend (Next.js)

```bash
# Navigate to frontend
cd Frontend

# Install dependencies
npm install

# Run development server (starts on http://localhost:3000)
npm run dev

# Build for production
npm run build

# Start production server
npm start

# Run linter
npm run lint
```

### Database Setup

1. Create the database:
```sql
CREATE DATABASE club_futbol_basico;
```

2. Run the initialization script:
```bash
# Execute init-database.sql in SQL Server
```

3. Update connection string in `Backend/API/appsettings.json`

## Architecture

### Backend Structure (Clean Architecture)

The backend follows Clean Architecture with four distinct layers:

**Domain Layer** (`Backend/Domain/`)
- Pure business entities with no external dependencies
- Contains all domain models: `Persona`, `Usuario`, `Socio`, `Rol`, `Actividad`, `Membresia`, `MembresiaActividad`, `Pago`, `MetodoPago`, `Asistencia`
- Entity relationships are established through navigation properties

**Application Layer** (`Backend/Application/`)
- `DTOs/`: Data Transfer Objects for API communication
- `Services/`: Business logic interfaces and implementations (`ISocioService`, `IAuthService`)
- Contains service interfaces that orchestrate domain logic

**Infrastructure Layer** (`Backend/Infrastructure/`)
- `Data/ClubDbContext.cs`: EF Core database context with complete entity configurations
- Maps domain entities to database tables using Fluent API
- Implements soft delete pattern via `FechaEliminacion`
- Auto-updates `FechaActualizacion` on entity modification (see ClubDbContext.SaveChangesAsync:237)

**API Layer** (`Backend/API/`)
- `Controllers/`: REST API endpoints (`AuthController`, `SociosController`)
- `Program.cs`: Application configuration including:
  - JWT Bearer authentication (Program.cs:22-36)
  - CORS policy for frontend (Program.cs:44-53)
  - Swagger/OpenAPI with JWT support (Program.cs:56-90)
  - Service registration with DI container

### Frontend Structure (Next.js App Router)

**Pages** (`Frontend/src/app/`)
- Uses Next.js 14 App Router (file-based routing)
- `login/`: Authentication page
- `dashboard/`: Protected dashboard with role-based access
- `layout.tsx`: Root layout with global providers

**API Layer** (`Frontend/src/lib/api/`)
- `client.ts`: Axios instance with JWT interceptor
- `auth.ts`: Authentication service (login, get current user)
- `socios.ts`: Member management service
- `actividades.ts`: Activity management service
- `membresias.ts`: Membership management service
- `pagos.ts`: Payment management service
- `usuarios.ts`: User management service
- All API calls include automatic token injection

**Dashboard Views** (`Frontend/src/app/dashboard/`)
- `socios/`: Member list, create, edit views
- `actividades/`: Activity list and create views
- `membresias/`: Membership list and create views (3-step wizard)
- `pagos/`: Payment list and register views (3-step wizard)
- `configuracion/roles/`: Role management under configuration section
- `configuracion/usuarios/`: User management views

### Database Naming Convention

**IMPORTANT**: The database uses snake_case for all column names, while C# entities use PascalCase. EF Core mappings handle this translation:
- Entity Property: `FechaCreacion` → Database Column: `fecha_creacion`
- Entity Property: `NombreUsuario` → Database Column: `nombre_usuario`

All table names are lowercase plural (e.g., `personas`, `usuarios`, `socios`, `membresias`).

### Key Domain Relationships

- `Persona` → One-to-One → `Usuario` (users for authentication)
- `Persona` → One-to-One → `Socio` (club members)
- `Usuario` → Many-to-One → `Rol` (user roles: superadmin, admin, recepcionista)
- `Socio` → One-to-Many → `Membresia` (monthly memberships)
- `Membresia` → Many-to-Many → `Actividad` via `MembresiaActividad` (membership activities with price snapshot)
- `Membresia` → One-to-Many → `Pago` (payments for memberships)
- `Pago` → Many-to-One → `MetodoPago` (payment method: efectivo, transferencia, etc.)
- `Pago` → Many-to-One → `Usuario` (user who registered the payment)
- `Socio` → One-to-Many → `Asistencia` (attendance tracking)

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

**Key Calculations**:
- `Membresia.TotalCargado` = Sum of all activity prices at the time of membership creation
- `Membresia.TotalPagado` = Sum of all payments for that membership
- `Membresia.Saldo` = TotalCargado - TotalPagado
- Membership is considered paid when Saldo <= 0

### Authentication & Authorization

**JWT Configuration**:
- JWT settings in `appsettings.json` (Jwt:Key, Jwt:Issuer, Jwt:Audience)
- Token includes user claims (user ID, username, role)
- Frontend stores token in localStorage
- API endpoints protected via `[Authorize]` attribute with role requirements

**Roles**:
- `superadmin`: Full system access, user management
- `admin`: Member management, payments, reports, attendance
- `recepcionista`: Read-only member access, attendance registration

### API Endpoints

All endpoints follow RESTful conventions. Below is the complete API documentation:

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
  - Query params: `idSocio`, `periodoAnio`, `periodoMes`, `soloImpagas`, `page`, `pageSize`
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
- `POST /api/usuarios` - Create user (superadmin only)
- `PUT /api/usuarios/{id}` - Update user (superadmin only)
- `PUT /api/usuarios/{id}/desactivar` - Deactivate user (superadmin only)

### Configuration Files

**Backend** (`Backend/API/appsettings.json`):
- Database connection string
- JWT configuration (Key must be at least 32 characters)
- Logging levels

**Frontend** (`.env.local` - create if missing):
```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

### Soft Delete Pattern

All entities implement soft delete via `FechaEliminacion` (nullable DateTime):
- When set, entity is considered deleted
- Queries should filter by `FechaEliminacion == null` for active records
- `Socio.EstaActivo` provides explicit active/inactive flag

### Business Rules

**Membership Management**:
- Cannot create duplicate memberships for same socio + period (periodoAnio + periodoMes)
- Memberships store activity prices at moment of creation (price snapshot in MembresiaActividad)
- Activities can be added/removed from memberships via special endpoints
- Removing activity is only allowed if no payments have been made
- Recepcionistas can assign/remove activities but cannot create/delete memberships

**Activity Management**:
- Activity prices are stored in each MembresiaActividad (historical pricing)
- Changing activity price doesn't affect existing memberships
- Soft delete prevents activity removal if used in active memberships

**Payment Registration**:
- Payments are always associated with a specific membership
- Multiple payments can be made for a single membership (partial payments)
- Payment receipt includes: socio info, membership details, payment method, amount, date
- Only admins and superadmins can register/void payments
- Payment amounts must be positive and cannot exceed membership saldo

### Development Notes

- API runs on `https://localhost:5000` with Swagger UI at root (`/`)
- Frontend runs on `http://localhost:3000`
- CORS is configured to allow requests from localhost:3000
- Database migrations are NOT used - schema is managed via `init-database.sql`
- All dates use SQL Server's `GETDATE()` for defaults
- Entity Framework tracks changes and auto-updates `FechaActualizacion` on save
- **Member numbers (numeroSocio)** are auto-generated by backend in SocioService.CrearAsync (format: SOC-0001, SOC-0002, etc.)
- CrearSocioDto does NOT include numeroSocio field - it's generated server-side
- Backend already filters inactive entities (FechaEliminacion != null), so frontend doesn't need additional filtering

### Form Validation Standards

**Frontend forms use React Hook Form + Zod with these validation rules:**
- **Nombre/Apellido**: Only letters (including accents and ñ), 2-50 chars, blocks numeric input via onKeyPress
- **DNI**: Only numbers, max 8 digits, blocks non-numeric input via onKeyPress
- **Email**: Standard email format, max 100 chars, auto-lowercase on submit
- **Fecha de Nacimiento**: Date type input with max set to today's date

### Frontend UI Patterns

**Step-by-Step Wizard Pattern**:
Complex forms use a multi-step wizard pattern for better UX:
- `membresias/nueva/page.tsx`: 3-step wizard (Select member → Select activities → Review and confirm)
  - Automatic total calculation based on selected activities
  - Real-time validation and step navigation
  - Prevents duplicate memberships for same period
- `pagos/nuevo/page.tsx`: 3-step wizard (Select member → Select unpaid membership → Payment details)
  - Dynamically loads payment methods from database (metodos_pago table)
  - Shows membership details and outstanding balance
  - Generates payment receipt on successful registration

**TailwindCSS Color Conventions**:
- Use `blue-*` classes for primary actions and highlights
- Use `green-*` for success states (paid, active)
- Use `red-*` for warning/error states (unpaid, inactive)
- Use `gray-*` for neutral elements and borders

### Important Field Changes

**Removed Fields** (no longer in use):
- `Actividad.EsCuotaBase` / `es_cuota_base` - Removed from entity, DTOs, services, and all UI components
  - All activities are now treated equally when calculating membership totals
  - No special "base fee" concept exists in the system

**Auto-Generated Fields**:
- `Socio.NumeroSocio` - Auto-generated in format SOC-0001, SOC-0002, etc.
  - Generated server-side by SocioService.CrearAsync
  - NOT included in CrearSocioDto
  - Sequential numbering based on max existing number + 1

### Test Credentials

Admin: `admin` / `admin123`
Recepcionista: `recepcionista` / `recep123`
