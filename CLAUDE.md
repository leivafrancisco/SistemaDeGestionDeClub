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
- All API calls include automatic token injection

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
- `Socio` → One-to-Many → `Asistencia` (attendance tracking)

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

### API Conventions

**Endpoints follow RESTful patterns**:
- `GET /api/socios` - List with pagination, filtering, and search
- `GET /api/socios/{id}` - Get by ID
- `GET /api/socios/numero/{numeroSocio}` - Get by member number
- `POST /api/socios` - Create (admin only) - Auto-generates numeroSocio in format SOC-0001, SOC-0002, etc.
- `PUT /api/socios/{id}` - Update (admin only)
- `PUT /api/socios/{id}/desactivar` - Soft delete (admin only)

**Query Parameters** (example: socios):
- `search`: Full-text search across nombre, apellido, email, dni, numero_socio
- `estaActivo`: Boolean filter for active/inactive
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20)

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

### Development Notes

- API runs on `https://localhost:5000` with Swagger UI at root (`/`)
- Frontend runs on `http://localhost:3000`
- CORS is configured to allow requests from localhost:3000
- Database migrations are NOT used - schema is managed via `init-database.sql`
- All dates use SQL Server's `GETDATE()` for defaults
- Entity Framework tracks changes and auto-updates `FechaActualizacion` on save
- **Member numbers (numeroSocio)** are auto-generated by backend in SocioService.CrearAsync (format: SOC-0001, SOC-0002, etc.)
- CrearSocioDto does NOT include numeroSocio field - it's generated server-side

### Form Validation Standards

**Frontend forms use React Hook Form + Zod with these validation rules:**
- **Nombre/Apellido**: Only letters (including accents and ñ), 2-50 chars, blocks numeric input via onKeyPress
- **DNI**: Only numbers, max 8 digits, blocks non-numeric input via onKeyPress
- **Email**: Standard email format, max 100 chars, auto-lowercase on submit
- **Fecha de Nacimiento**: Date type input with max set to today's date

### Test Credentials

Admin: `admin` / `admin123`
Recepcionista: `recepcionista` / `recep123`
