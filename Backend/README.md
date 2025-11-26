# Backend - Sistema de Gestión de Club de Fútbol

## ¿Qué es el Backend?

El backend es el "cerebro" de la aplicación. Es la parte del sistema que:
- Se conecta a la base de datos
- Procesa la lógica de negocio (reglas y operaciones)
- Proporciona endpoints (URLs) para que el frontend solicite datos
- Maneja la autenticación y seguridad

**Tecnología**: ASP.NET Core 8 Web API con C#

---

## Arquitectura del Proyecto

El backend sigue el patrón **Clean Architecture** (Arquitectura Limpia), que separa el código en 4 capas independientes:

```
Backend/
│
├── Domain/                    # Capa 1: DOMINIO
│   └── Entities/             # Modelos de datos puros (Socio, Usuario, Membresia, etc.)
│
├── Application/              # Capa 2: APLICACIÓN
│   ├── DTOs/                # Objetos para transferir datos (entrada/salida de la API)
│   └── Services/            # Lógica de negocio (ISocioService, IAuthService)
│
├── Infrastructure/          # Capa 3: INFRAESTRUCTURA
│   └── Data/               # Conexión a base de datos (Entity Framework Core)
│       └── ClubDbContext.cs
│
└── API/                    # Capa 4: API (Punto de entrada)
    ├── Controllers/        # Endpoints HTTP (AuthController, SociosController)
    ├── Program.cs         # Configuración de la aplicación
    └── appsettings.json   # Configuración (base de datos, JWT, etc.)
```

### ¿Por qué esta arquitectura?

- **Domain**: Contiene las entidades del negocio sin dependencias externas
- **Application**: Contiene la lógica de negocio y define "qué" hace el sistema
- **Infrastructure**: Implementa "cómo" se hacen las cosas (base de datos, archivos, etc.)
- **API**: Expone los endpoints HTTP para que el frontend pueda comunicarse

---

## Requisitos Previos

### 1. .NET 8 SDK

**Verificar si ya lo tienes instalado:**
```bash
dotnet --version
```

**Si muestra**: `8.x.x` → Ya lo tienes instalado
**Si muestra un error**: Necesitas instalarlo desde [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

### 2. SQL Server

Necesitas SQL Server 2022 (o superior) instalado y corriendo.

**Opciones de instalación:**
- SQL Server Express (gratis): [Descargar aquí](https://www.microsoft.com/es-es/sql-server/sql-server-downloads)
- SQL Server Management Studio (SSMS) para administrar la base de datos

### 3. Editor de código (opcional pero recomendado)

- Visual Studio 2022 Community (gratis)
- Visual Studio Code con extensión C#
- Rider (de pago)

---

## Configuración Paso a Paso

### Paso 1: Crear la Base de Datos

1. Abre **SQL Server Management Studio (SSMS)**
2. Conéctate a tu servidor local (normalmente `localhost` o `.\SQLEXPRESS`)
3. Ejecuta el script de inicialización:

```sql
-- Primero crea la base de datos
CREATE DATABASE club_futbol_basico;
GO

-- Luego ejecuta el archivo init-database.sql que está en la raíz del proyecto
-- (Cópialo y pégalo en SSMS, o ábrelo desde el menú File > Open > File)
```

**Ubicación del script**: `../init-database.sql`

### Paso 2: Configurar la Conexión a la Base de Datos

1. Ve a `Backend/API/appsettings.json`
2. Modifica la cadena de conexión según tu configuración de SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=NOMBRE_DE_TU_SERVIDOR;Database=club_futbol_basico;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True;"
  }
}
```

**Ejemplos comunes de cadenas de conexión:**

**SQL Server Express con Windows Authentication:**
```
Server=localhost\\SQLEXPRESS;Database=club_futbol_basico;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server con usuario sa:**
```
Server=localhost;Database=club_futbol_basico;User Id=sa;Password=TuContraseña123!;TrustServerCertificate=True;
```

### Paso 3: Verificar la Configuración JWT

El archivo `appsettings.json` también contiene la configuración de JWT (autenticación):

```json
{
  "Jwt": {
    "Key": "TuClaveSecretaSuperSeguraDeAlMenos32Caracteres!",
    "Issuer": "SistemaDeGestionDeClub",
    "Audience": "SistemaDeGestionDeClub"
  }
}
```

**No necesitas cambiar esto**, a menos que quieras usar tu propia clave secreta.

---

## Ejecutar el Backend

### Opción 1: Desde la Terminal (Recomendado para aprender)

1. Abre una terminal en la carpeta del proyecto
2. Navega a la carpeta del proyecto API:

```bash
cd Backend/API
```

3. Restaura las dependencias (paquetes NuGet):

```bash
dotnet restore
```

4. Compila el proyecto:

```bash
dotnet build
```

5. Ejecuta el proyecto:

```bash
dotnet run
```

**Salida esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5000
      Now listening on: http://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Opción 2: Modo Watch (Auto-recarga al hacer cambios)

Si quieres que el backend se reinicie automáticamente cada vez que guardes cambios en el código:

```bash
dotnet watch run
```

### Opción 3: Desde Visual Studio

1. Abre la solución `Backend/SistemaDeGestionDeClub.sln`
2. Establece `API` como proyecto de inicio (clic derecho → Set as Startup Project)
3. Presiona **F5** o el botón **▶ Start**

---

## Probar el Backend

### Método 1: Swagger UI (La forma más fácil)

Swagger es una interfaz web que te permite probar todos los endpoints sin escribir código.

1. **Inicia el backend** (usando uno de los métodos anteriores)
2. **Abre tu navegador** en: [https://localhost:5000](https://localhost:5000)
3. Verás la **interfaz de Swagger** con todos los endpoints disponibles

#### Ejemplo: Probar el Login

1. En Swagger, busca la sección **Auth**
2. Haz clic en `POST /api/Auth/login` → **Try it out**
3. Ingresa el siguiente JSON en el campo de texto:

```json
{
  "nombreUsuario": "admin",
  "password": "admin123"
}
```

4. Haz clic en **Execute**
5. Verás la respuesta:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": 1,
    "nombreUsuario": "admin",
    "nombre": "Administrador",
    "apellido": "Sistema",
    "rol": "admin"
  }
}
```

6. **Copia el token** (el texto largo que empieza con `eyJ...`)

#### Autenticación en Swagger

Para probar endpoints protegidos (que requieren autenticación):

1. Haz clic en el botón **Authorize** (arriba a la derecha)
2. Ingresa: `Bearer TU_TOKEN_AQUI` (reemplaza `TU_TOKEN_AQUI` con el token que copiaste)
3. Haz clic en **Authorize** y luego **Close**
4. Ahora puedes probar endpoints protegidos como `GET /api/Socios`

### Método 2: Postman o Insomnia

Si prefieres usar una herramienta de prueba de APIs:

#### Probar Login

**Request:**
```
POST https://localhost:5000/api/Auth/login
Content-Type: application/json

{
  "nombreUsuario": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": { ... }
}
```

#### Probar Obtener Socios (requiere autenticación)

**Request:**
```
GET https://localhost:5000/api/Socios
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Método 3: cURL (Terminal)

#### Login:
```bash
curl -X POST https://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d "{\"nombreUsuario\":\"admin\",\"password\":\"admin123\"}" \
  -k
```

#### Obtener socios:
```bash
curl -X GET https://localhost:5000/api/Socios \
  -H "Authorization: Bearer TU_TOKEN_AQUI" \
  -k
```

---

## Endpoints Disponibles

### Autenticación (Auth)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| POST | `/api/Auth/login` | Iniciar sesión | No |
| GET | `/api/Auth/me` | Obtener usuario actual | Sí |

### Socios

| Método | Endpoint | Descripción | Rol Requerido |
|--------|----------|-------------|---------------|
| GET | `/api/Socios` | Listar todos los socios (con filtros) | Cualquier usuario autenticado |
| GET | `/api/Socios/{id}` | Obtener socio por ID | Cualquier usuario autenticado |
| GET | `/api/Socios/numero/{numeroSocio}` | Obtener socio por número | Cualquier usuario autenticado |
| POST | `/api/Socios` | Crear nuevo socio | admin, superadmin |
| PUT | `/api/Socios/{id}` | Actualizar socio | admin, superadmin |
| PUT | `/api/Socios/{id}/desactivar` | Desactivar (dar de baja) socio | admin, superadmin |
| GET | `/api/Socios/estadisticas/total` | Total de socios activos | Cualquier usuario autenticado |

### Parámetros de Query para GET /api/Socios

- `search`: Búsqueda de texto (nombre, apellido, email, dni, número de socio)
- `estaActivo`: Filtrar por activos (`true`) o inactivos (`false`)
- `page`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 20)

**Ejemplo:**
```
GET /api/Socios?search=juan&estaActivo=true&page=1&pageSize=10
```

### Health Check

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/health` | Verificar que el servidor está activo |

---

## Usuarios de Prueba

La base de datos ya viene con usuarios creados:

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| admin | admin123 | admin |
| recepcionista | recep123 | recepcionista |

**Roles y Permisos:**

- **superadmin**: Acceso completo al sistema
- **admin**: Gestión de socios, pagos, reportes, asistencias
- **recepcionista**: Solo lectura de socios y registro de asistencias

---

## Solución de Problemas Comunes

### Error: "Unable to connect to database"

**Causa**: La cadena de conexión es incorrecta o SQL Server no está corriendo.

**Solución:**
1. Verifica que SQL Server esté corriendo
2. Revisa la cadena de conexión en `appsettings.json`
3. Intenta conectarte a la base de datos desde SSMS con las mismas credenciales

### Error: "SecurityTokenException: IDX10503"

**Causa**: El token JWT es inválido o expiró.

**Solución:**
1. Vuelve a hacer login para obtener un nuevo token
2. Asegúrate de incluir `Bearer ` antes del token en el header Authorization

### Error: "The SSL connection could not be established"

**Causa**: Problemas con certificados SSL en desarrollo.

**Solución:**
Agrega `TrustServerCertificate=True;` a tu cadena de conexión, o usa HTTP en lugar de HTTPS:
```bash
dotnet run --urls "http://localhost:5000"
```

### Puerto en uso

**Error**: "Failed to bind to address https://localhost:5000: address already in use"

**Solución:**
1. Cierra la aplicación que está usando el puerto
2. O cambia el puerto en `appsettings.json` (en la sección `Kestrel`)

### Swagger no carga

**Causa**: Solo está disponible en modo Development.

**Solución:**
Verifica que la variable de entorno `ASPNETCORE_ENVIRONMENT` esté en `Development`:

```bash
$env:ASPNETCORE_ENVIRONMENT="Development"  # PowerShell
set ASPNETCORE_ENVIRONMENT=Development     # CMD
export ASPNETCORE_ENVIRONMENT=Development  # Linux/Mac
```

---

## Conceptos Clave para Entender

### ¿Qué es una API REST?

Es una forma de comunicación entre aplicaciones usando HTTP. Cada endpoint (URL) representa un recurso:

- `GET /api/Socios` → Obtener lista de socios
- `POST /api/Socios` → Crear un nuevo socio
- `PUT /api/Socios/1` → Actualizar el socio con ID 1

### ¿Qué es JWT (JSON Web Token)?

Es un token de autenticación. Cuando haces login, el servidor te devuelve un token que debes incluir en cada petición:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### ¿Qué es Entity Framework Core?

Es un ORM (Object-Relational Mapper) que traduce tus clases de C# a tablas de base de datos y viceversa.

**Ejemplo:**
```csharp
// En lugar de escribir SQL:
SELECT * FROM socios WHERE id = 1;

// Escribes C#:
var socio = await context.Socios.FindAsync(1);
```

### ¿Qué es Dependency Injection?

Es un patrón que permite inyectar dependencias en clases. Por ejemplo, en `SociosController`:

```csharp
public SociosController(ISocioService socioService)
{
    _socioService = socioService; // Se inyecta automáticamente
}
```

Esto se configura en `Program.cs`:
```csharp
builder.Services.AddScoped<ISocioService, SocioService>();
```

---

## Siguientes Pasos

1. **Explora el código**: Abre `Controllers/SociosController.cs` y lee los comentarios
2. **Experimenta con Swagger**: Prueba todos los endpoints
3. **Revisa los DTOs**: Ve a `Application/DTOs` para ver qué datos recibe/envía la API
4. **Aprende sobre Entity Framework**: Revisa `Infrastructure/Data/ClubDbContext.cs`

---

## Recursos de Aprendizaje

- [Documentación oficial de ASP.NET Core](https://learn.microsoft.com/es-es/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/es-es/ef/core/)
- [REST API Tutorial](https://restfulapi.net/)
- [JWT.io](https://jwt.io/) - Decodificador de tokens JWT

---

## Contacto

Si tienes dudas sobre el código o la arquitectura, revisa los comentarios en el código o consulta el archivo `CLAUDE.md` en la raíz del proyecto.
