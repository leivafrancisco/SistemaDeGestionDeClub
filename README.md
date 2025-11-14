# Sistema de Gestión de Club de Fútbol

Sistema completo de gestión para clubes deportivos con backend en ASP.NET Core 8 y frontend en Next.js 14.

## 🏗️ Arquitectura

### Backend
- **Framework**: ASP.NET Core 8 Web API
- **Base de datos**: SQL Server 2022
- **ORM**: Entity Framework Core 8
- **Autenticación**: JWT Bearer
- **Patrón**: Clean Architecture

### Frontend
- **Framework**: Next.js 14 (App Router)
- **UI**: React 18 + TypeScript
- **Estilos**: TailwindCSS
- **HTTP Client**: Axios
- **Validación**: Zod + React Hook Form

## 📋 Requisitos Previos

- .NET 8 SDK
- Node.js 18+ y npm
- SQL Server 2022 (o SQL Server Express)
- Visual Studio Code o Visual Studio 2022

## 🚀 Instalación y Configuración

### 1. Clonar el repositorio

```bash
git clone <tu-repositorio>
cd SistemaDeGestionDeClub
```

### 2. Configurar Base de Datos

#### Crear la base de datos en SQL Server

```sql
CREATE DATABASE club_futbol_basico;
```

#### Ejecutar el script de inicialización (ver init-database.sql)

Este script creará todas las tablas y datos iniciales.

### 3. Configurar Backend

#### Actualizar connection string

Editar `Backend/API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=club_futbol_basico;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
  }
}
```

#### Restaurar paquetes y ejecutar

```bash
cd Backend/API
dotnet restore
dotnet build
dotnet run
```

El API estará disponible en: `https://localhost:5000`
Swagger UI: `https://localhost:5000` (raíz)

### 4. Configurar Frontend

#### Instalar dependencias

```bash
cd Frontend
npm install
```

#### Configurar variable de entorno

Crear archivo `.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

#### Ejecutar en modo desarrollo

```bash
npm run dev
```

El frontend estará disponible en: `http://localhost:3000`

## 👤 Credenciales de Prueba

### Usuario Administrador
- **Usuario**: `admin`
- **Contraseña**: `admin123`
- **Rol**: admin

### Usuario Recepcionista
- **Usuario**: `recepcionista`
- **Contraseña**: `recep123`
- **Rol**: recepcionista

## 📁 Estructura del Proyecto

```
SistemaDeGestionDeClub/
├── Backend/
│   ├── Domain/
│   │   └── Entities/          # Entidades del dominio
│   ├── Application/
│   │   ├── DTOs/              # Data Transfer Objects
│   │   └── Services/          # Lógica de negocio
│   ├── Infrastructure/
│   │   └── Data/              # DbContext y configuración EF
│   └── API/
│       ├── Controllers/       # Controladores REST
│       ├── Program.cs         # Configuración de la app
│       └── appsettings.json   # Configuración
│
└── Frontend/
    └── src/
        ├── app/               # Páginas (App Router)
        │   ├── login/         # Página de login
        │   ├── dashboard/     # Dashboard principal
        │   └── layout.tsx     # Layout raíz
        └── lib/
            └── api/           # Servicios API
```

## 🔑 Endpoints Principales del API

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `GET /api/auth/me` - Obtener usuario actual

### Socios
- `GET /api/socios` - Listar socios (con filtros y paginación)
- `GET /api/socios/{id}` - Obtener socio por ID
- `GET /api/socios/numero/{numeroSocio}` - Buscar por número de socio
- `POST /api/socios` - Crear nuevo socio (admin)
- `PUT /api/socios/{id}` - Actualizar socio (admin)
- `PUT /api/socios/{id}/desactivar` - Dar de baja (admin)
- `GET /api/socios/estadisticas/total` - Total de socios activos

### Parámetros de búsqueda (Socios)
- `search`: Buscar por nombre, apellido, email, DNI o número de socio
- `estaActivo`: Filtrar por estado (true/false)
- `page`: Número de página (default: 1)
- `pageSize`: Elementos por página (default: 20)

## 🛡️ Roles y Permisos

### Superadmin
- Acceso completo a todas las funcionalidades
- Crear y modificar usuarios
- Gestionar roles

### Admin
- Gestión completa de socios
- Procesamiento de pagos
- Generación de reportes
- Registro de asistencias

### Recepcionista
- Consulta de socios
- Registro de asistencias
- Vista limitada del dashboard

## 🧪 Características Implementadas

✅ Sistema de autenticación con JWT
✅ Gestión completa de socios (CRUD)
✅ Búsqueda y filtrado de socios
✅ Dashboard con estadísticas
✅ Control de acceso basado en roles
✅ Interfaz responsive con TailwindCSS
✅ Validación de formularios
✅ Manejo de errores y estados de carga

## 📝 Próximas Funcionalidades (Roadmap)

- [ ] Gestión de membresías mensuales
- [ ] Procesamiento de pagos
- [ ] Registro de asistencias con check-in
- [ ] Módulo de reportes y estadísticas
- [ ] Notificaciones de pagos vencidos
- [ ] Exportación de datos (Excel, PDF)
- [ ] Panel de reportes avanzados
- [ ] Gestión de actividades
- [ ] Calendario de eventos

## 🔧 Tecnologías Utilizadas

### Backend
- ASP.NET Core 8
- Entity Framework Core 8
- SQL Server
- JWT Bearer Authentication
- Swagger/OpenAPI

### Frontend
- Next.js 14
- React 18
- TypeScript
- TailwindCSS
- Axios
- React Hook Form
- Zod
- date-fns
- Lucide Icons

## 📞 Soporte

Para reportar bugs o solicitar nuevas características, por favor crea un issue en el repositorio.

## 📄 Licencia

Este proyecto es privado y confidencial.
