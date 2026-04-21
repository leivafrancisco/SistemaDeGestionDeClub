# Sistema de Gestión de Club de Fútbol

Sistema completo de gestión para clubes deportivos con backend en ASP.NET Core 8 y frontend en Next.js 14.

## 🏗️ Arquitectura

### Backend
- **Framework**: ASP.NET Core 8 Web API
- **Base de datos**: Supabase - Postgres SQL 
- **ORM**: Entity Framework Core 8
- **Autenticación**: JWT Bearer
- **Patrón**: Clean Architecture

### Frontend
- **Framework**: Next.js 14 (App Router)
- **UI**: React 18 + TypeScript
- **Estilos**: Tailwind CSS
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

### Superadmin
- **Usuario**: `superadmin`
- **Contraseña**: `super123`
- **Rol**: superadmin

### Administrador
- **Usuario**: `admin`
- **Contraseña**: `admin123`
- **Rol**: admin

### Recepcionista
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

## 💬 Flujo: Crear Membresía

**Contexto:** Andrés (admin) renueva la membresía de un socio que viene a pagar, con pago parcial inicial.

---

**1. Buscar el socio**

```
GET /api/socios?search=38521047
```

```json
{
  "id": 42,
  "numeroSocio": "SOC-0042",
  "nombre": "Carlos",
  "apellido": "Rodríguez",
  "estaActivo": true
}
```

---

**2. Ver actividades disponibles**

```
GET /api/actividades
```

```json
[
  { "id": 1, "nombre": "Fútbol",   "precio": 8000.00 },
  { "id": 2, "nombre": "Gimnasio", "precio": 6000.00 },
  { "id": 3, "nombre": "Natación", "precio": 7000.00 }
]
```

> El socio elige Fútbol y Gimnasio. Puede abonar $10.000 hoy y el resto después.

---

**3. Crear membresía con pago parcial**

```
POST /api/membresias
```

```json
{
  "idSocio": 42,
  "fechaInicio": "2026-04-16",
  "fechaFin": "2026-05-15",
  "idsActividades": [1, 2],
  "costoTotal": 14000.00,
  "monto": 10000.00,
  "idMetodoPago": 1
}
```

> `costoTotal` = total de la membresía · `monto` = lo que abona hoy (puede ser parcial)

**El sistema internamente:**
1. Valida fechas y que no haya solapamiento con otra membresía del mismo socio
2. Inserta la membresía y congela los precios de cada actividad en `membresia_actividades`
3. Registra el pago inicial en `pagos`
4. Calcula: `saldo = costoTotal - monto = 14000 - 10000 = 4000`

```json
{
  "id": 210,
  "numeroSocio": "SOC-0042",
  "fechaInicio": "2026-04-16",
  "fechaFin": "2026-05-15",
  "actividades": [
    { "nombre": "Fútbol",   "precioAlMomento": 8000.00 },
    { "nombre": "Gimnasio", "precioAlMomento": 6000.00 }
  ],
  "costoTotal": 14000.00,
  "totalPagado": 10000.00,
  "saldo": 4000.00,
  "estaPaga": false
}
```

---

**4. Pagar el saldo restante (cuando el socio trae el resto)**

```
POST /api/pagos
```

```json
{ "idMembresia": 210, "idMetodoPago": 1, "monto": 4000.00 }
```

```json
{
  "numeroPago": "PAG-000315-2026",
  "monto": 4000.00,
  "saldoAntes": 4000.00,
  "saldoDespues": 0.00,
  "estaPaga": true
}
```

---

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
