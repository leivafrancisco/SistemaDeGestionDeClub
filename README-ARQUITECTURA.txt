================================================================================
                SISTEMA DE GESTIÓN DE CLUB DE FÚTBOL
           DOCUMENTACIÓN COMPLETA DE ARQUITECTURA Y FUNCIONALIDADES
================================================================================

ÍNDICE
------
1. Visión General del Sistema
2. Arquitectura del Proyecto
3. Modelo de Dominio (Entidades)
4. Funcionalidades por Módulo
5. Cambios Importantes Recientes
6. Reglas de Negocio
7. Patrones de Diseño Utilizados
8. Flujos Principales del Sistema

================================================================================
1. VISIÓN GENERAL DEL SISTEMA
================================================================================

El Sistema de Gestión de Club de Fútbol es una aplicación web completa que
permite administrar:
  - Socios del club
  - Usuarios del sistema (administradores y recepcionistas)
  - Membresías con actividades
  - Pagos y facturación
  - Asistencias
  - Actividades deportivas

STACK TECNOLÓGICO:
  Backend:  ASP.NET Core 8 Web API (Clean Architecture)
  Frontend: Next.js 14 con TypeScript y TailwindCSS
  Base de datos: SQL Server 2022
  ORM: Entity Framework Core 8
  Autenticación: JWT (JSON Web Tokens)

================================================================================
2. ARQUITECTURA DEL PROYECTO
================================================================================

El backend sigue Clean Architecture con 4 capas bien definidas:

┌─────────────────────────────────────────────────────────────────┐
│                        API LAYER                                 │
│  - Controllers (REST endpoints)                                  │
│  - Program.cs (configuración, DI, middleware)                    │
│  - Swagger/OpenAPI                                               │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                             │
│  - DTOs (Data Transfer Objects)                                  │
│  - Services (lógica de negocio)                                  │
│  - Interfaces de servicios                                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                  INFRASTRUCTURE LAYER                            │
│  - ClubDbContext (EF Core)                                       │
│  - Configuraciones de entidades (Fluent API)                     │
│  - Acceso a datos                                                │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                      DOMAIN LAYER                                │
│  - Entidades del dominio (POCOs)                                 │
│  - Propiedades calculadas                                        │
│  - Sin dependencias externas                                     │
└─────────────────────────────────────────────────────────────────┘

PRINCIPIOS APLICADOS:
  ✓ Separation of Concerns
  ✓ Dependency Inversion
  ✓ Single Responsibility
  ✓ DRY (Don't Repeat Yourself)

================================================================================
3. MODELO DE DOMINIO (ENTIDADES)
================================================================================

3.1. ROL
--------
Roles del sistema para control de acceso.

CAMPOS:
  - id: int (PK)
  - nombre: string (unique)
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable, soft delete)

ROLES PREDEFINIDOS:
  • superadmin: Acceso total, puede crear usuarios admin
  • admin: Gestión de socios, membresías, pagos, actividades
  • recepcionista: Solo lectura de socios, registro de asistencias

RELACIONES:
  - Un Rol tiene muchos Usuarios


3.2. PERSONA
------------
Información personal base para Usuarios y Socios.

CAMPOS:
  - id: int (PK)
  - nombre: string (2-50 caracteres, solo letras)
  - apellido: string (2-50 caracteres, solo letras)
  - email: string (único, max 100 caracteres)
  - dni: string (max 8 dígitos)
  - fecha_nacimiento: date
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable)

PROPIEDADES CALCULADAS:
  - NombreCompleto: string → "{nombre} {apellido}"

VALIDACIONES FRONTEND:
  ✓ Nombre/Apellido: Solo letras (incluye acentos y ñ)
  ✓ DNI: Solo números, máximo 8 dígitos
  ✓ Email: Formato válido, auto-lowercase
  ✓ Fecha de nacimiento: No puede ser futura

RELACIONES:
  - Una Persona puede ser un Usuario (1:0..1)
  - Una Persona puede ser un Socio (1:0..1)


3.3. USUARIO
------------
Usuarios del sistema que pueden autenticarse.

CAMPOS:
  - id: int (PK)
  - id_persona: int (FK → personas.id)
  - id_rol: int (FK → roles.id)
  - nombre_usuario: string (único)
  - contrasena_hash: string
  - esta_activo: bool
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable)

FUNCIONALIDADES (UsuarioService):
  ✓ CrearAsync(dto, rolCreador): Crear usuario
    • Admin puede crear solo recepcionistas
    • Superadmin puede crear admin o recepcionistas
    • Valida unicidad de nombre_usuario y email
    • Auto-asigna rol según permisos del creador

  ✓ ObtenerPorIdAsync(id): Obtener usuario por ID
  ✓ ObtenerTodosAsync(rol, estaActivo): Listar con filtros
  ✓ ActualizarAsync(id, dto): Actualizar datos
  ✓ DesactivarAsync(id): Soft delete

SEGURIDAD:
  - Contraseñas en texto plano (SOLO DESARROLLO)
    ⚠️ PRODUCCIÓN: Implementar BCrypt.Net
  - JWT con claims: userId, username, email, role
  - Tokens expiran en 24 horas

RELACIONES:
  - Usuario pertenece a una Persona (n:1)
  - Usuario tiene un Rol (n:1)
  - Usuario procesa muchos Pagos (1:n)


3.4. SOCIO
----------
Miembros del club que pueden tener membresías.

CAMPOS:
  - id: int (PK)
  - id_persona: int (FK → personas.id)
  - numero_socio: string (único, auto-generado)
  - esta_activo: bool
  - fecha_alta: datetime
  - fecha_baja: datetime (nullable)
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable)

FUNCIONALIDADES (SocioService):
  ✓ CrearAsync(dto): Crear socio
    • AUTO-GENERA numero_socio (formato: SOC-0001, SOC-0002...)
    • Valida unicidad de email y DNI
    • Crea Persona asociada automáticamente
    • fecha_alta se establece al momento de creación

  ✓ ObtenerPorIdAsync(id): Obtener por ID
  ✓ ObtenerTodosAsync(filtros): Listar con filtros
    • Filtros: search (nombre, dni, email, numero_socio), estaActivo
    • Paginación incluida

  ✓ ActualizarAsync(id, dto): Actualizar datos
  ✓ DesactivarAsync(id): Soft delete (marca fecha_baja)
  ✓ ContarTotalAsync(): Total de socios activos

REGLAS:
  - El numero_socio NO se incluye en CrearSocioDto
  - Es generado automáticamente por el backend
  - Formato secuencial: SOC-0001, SOC-0002, etc.

RELACIONES:
  - Socio pertenece a una Persona (n:1)
  - Socio tiene muchas Membresías (1:n)
  - Socio tiene muchas Asistencias (1:n)


3.5. ACTIVIDAD
--------------
Actividades deportivas que se pueden asignar a membresías.

CAMPOS:
  - id: int (PK)
  - nombre: string (único)
  - descripcion: string (nullable)
  - precio: decimal(10,2)
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable)

FUNCIONALIDADES (ActividadService):
  ✓ ObtenerTodas(): Listar todas las activas
  ✓ ObtenerPorId(id): Obtener por ID
  ✓ Crear(dto): Crear actividad
  ✓ Actualizar(id, dto): Actualizar (nombre, descripción, precio)
  ✓ Eliminar(id): Soft delete

IMPORTANTE:
  ❌ ELIMINADO: Campo "es_cuota_base"
    Ya no existe el concepto de cuota base
    Todas las actividades son iguales

PRICING:
  - El precio de la actividad se captura en MembresiaActividad
  - Cambiar el precio NO afecta membresías existentes
  - Cada membresía guarda el precio histórico

RELACIONES:
  - Actividad se usa en muchas MembresiaActividad (1:n)


3.6. MEMBRESIA
--------------
Períodos de membresía de un socio con actividades asignadas.

CAMPOS:
  - id: int (PK)
  - id_socio: int (FK → socios.id)
  - fecha_inicio: date ← ACTUALIZADO
  - fecha_fin: date ← ACTUALIZADO
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable)

CAMPOS ELIMINADOS:
  ❌ periodo_anio: short
  ❌ periodo_mes: byte

PROPIEDADES CALCULADAS:
  - TotalCargado: decimal → Suma de precios en membresia_actividades
  - TotalPagado: decimal → Suma de pagos asociados
  - Saldo: decimal → TotalCargado - TotalPagado

FUNCIONALIDADES (MembresiaService):
  ✓ ObtenerTodosAsync(filtros): Listar con filtros
    • Filtros: idSocio, fechaDesde, fechaHasta, soloImpagas
    • Ordenado por fechaInicio DESC

  ✓ ObtenerPorIdAsync(id): Obtener detalle completo
  ✓ CrearAsync(dto): Crear membresía
    • Valida que fecha_fin > fecha_inicio
    • Valida que no haya solapamiento de fechas para el mismo socio
    • Asocia actividades con precio histórico

  ✓ ActualizarAsync(id, dto): Actualizar actividades
    • Solo si NO tiene pagos registrados

  ✓ EliminarAsync(id): Soft delete
    • Solo si NO tiene pagos registrados

  ✓ AsignarActividadAsync(dto): Agregar actividad a membresía
    • Captura precio actual de la actividad

  ✓ RemoverActividadAsync(dto): Quitar actividad
    • Solo si la membresía NO tiene pagos

  ✓ ContarTotalAsync(): Total de membresías activas

REGLAS DE NEGOCIO:
  ✓ No se permiten fechas solapadas para el mismo socio
  ✓ Fecha fin debe ser posterior a fecha inicio
  ✓ Las actividades se pueden modificar hasta el primer pago
  ✓ El total se calcula automáticamente

RELACIONES:
  - Membresía pertenece a un Socio (n:1)
  - Membresía tiene muchas MembresiaActividad (1:n)
  - Membresía tiene muchos Pagos (1:n)


3.7. MEMBRESIA_ACTIVIDAD
------------------------
Tabla intermedia que captura el snapshot de precios.

CAMPOS:
  - id_membresia: int (PK, FK → membresias.id)
  - id_actividad: int (PK, FK → actividades.id)
  - precio_al_momento: decimal(10,2)

PROPÓSITO:
  Esta tabla captura el precio de la actividad EN EL MOMENTO
  de crear la membresía. Esto permite:
    ✓ Historial de precios
    ✓ Cambiar precios sin afectar membresías existentes
    ✓ Cálculos precisos de totales

RELACIONES:
  - Pertenece a una Membresía (n:1)
  - Referencia a una Actividad (n:1)


3.8. METODO_PAGO
----------------
Métodos de pago disponibles en el sistema.

CAMPOS:
  - id: int (PK)
  - nombre: string (único, ej: "Efectivo", "Transferencia", "Tarjeta")

MÉTODOS COMUNES:
  • Efectivo
  • Transferencia
  • Tarjeta de Débito
  • Tarjeta de Crédito
  • Mercado Pago

NOTA: Se gestiona directamente en la base de datos.

RELACIONES:
  - MetodoPago se usa en muchos Pagos (1:n)


3.9. PAGO
---------
Registros de pagos realizados por los socios.

CAMPOS:
  - id: int (PK)
  - id_membresia: int (FK → membresias.id)
  - id_metodo_pago: int (FK → metodos_pago.id)
  - id_usuario_procesa: int (FK → usuarios.id, nullable)
  - monto: decimal(10,2)
  - fecha_pago: datetime
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable)

FUNCIONALIDADES (PagoService):
  ✓ RegistrarAsync(dto): Registrar nuevo pago
    • Valida que la membresía exista
    • Valida que el monto sea positivo
    • Actualiza saldo de la membresía
    • Genera comprobante automáticamente

  ✓ ObtenerPorIdAsync(id): Obtener pago por ID
  ✓ ObtenerTodosAsync(filtros): Listar con filtros
    • Filtros: idMembresia, idSocio, idMetodoPago, fechaDesde, fechaHasta

  ✓ GenerarComprobanteAsync(id): Generar comprobante PDF/digital
    • Formato: PAG-XXXXXX-YYYY
    • Incluye: datos socio, membresía, pago, saldos

  ✓ AnularAsync(id): Anular pago (soft delete)
    • Recalcula saldo de membresía

  ✓ ObtenerMetodosPagoAsync(): Listar métodos disponibles
  ✓ ObtenerEstadisticasAsync(filtros): Estadísticas de pagos

COMPROBANTE DE PAGO:
  Número: PAG-{id:6 dígitos}-{año}
  Ejemplo: PAG-000123-2025

  Incluye:
    - Datos del socio (número, nombre)
    - Período de membresía (rango de fechas)
    - Total de la membresía
    - Total pagado antes
    - Monto del pago actual
    - Nuevo saldo
    - Método de pago
    - Fecha y hora
    - Usuario que procesó

REGLAS:
  ✓ El monto no puede exceder el saldo de la membresía
  ✓ Se permiten pagos parciales
  ✓ Un pago se asocia a UNA membresía
  ✓ El usuario que procesa queda registrado

RELACIONES:
  - Pago pertenece a una Membresía (n:1)
  - Pago usa un MetodoPago (n:1)
  - Pago es procesado por un Usuario (n:1)


3.10. ASISTENCIA
----------------
Registro de asistencias de socios al club.

CAMPOS:
  - id: int (PK)
  - id_socio: int (FK → socios.id)
  - fecha_hora_ingreso: datetime
  - fecha_creacion: datetime
  - fecha_actualizacion: datetime
  - fecha_eliminacion: datetime (nullable)

FUNCIONALIDAD:
  Registra cuando un socio ingresa al club.
  Útil para estadísticas de uso.

RELACIONES:
  - Asistencia pertenece a un Socio (n:1)

================================================================================
4. FUNCIONALIDADES POR MÓDULO
================================================================================

4.1. MÓDULO DE AUTENTICACIÓN
-----------------------------
Archivo: AuthService.cs

ENDPOINTS:
  POST   /api/auth/login
  GET    /api/auth/me

FUNCIONALIDADES:
  ✓ LoginAsync(dto)
    • Valida credenciales
    • Genera token JWT
    • Retorna: token + datos usuario

  ✓ ObtenerUsuarioActualAsync(userId)
    • Obtiene datos del usuario autenticado
    • Incluye: nombre completo, email, rol

SEGURIDAD:
  - JWT Bearer authentication
  - Claims en token: userId, username, email, role
  - Expiración: 24 horas
  - Issuer/Audience configurables


4.2. MÓDULO DE USUARIOS
------------------------
Archivo: UsuarioService.cs

ENDPOINTS:
  GET    /api/usuarios                 [admin, superadmin]
  GET    /api/usuarios/{id}            [admin, superadmin]
  POST   /api/usuarios                 [admin, superadmin]
  PUT    /api/usuarios/{id}            [superadmin]
  PUT    /api/usuarios/{id}/desactivar [superadmin]

PERMISOS ESPECIALES:
  ✓ Admin puede crear SOLO recepcionistas
  ✓ Superadmin puede crear admins y recepcionistas
  ✓ Solo superadmin puede editar/desactivar usuarios


4.3. MÓDULO DE SOCIOS
----------------------
Archivo: SocioService.cs

ENDPOINTS:
  GET    /api/socios                   [todos los roles]
  GET    /api/socios/{id}              [todos los roles]
  GET    /api/socios/numero/{num}      [todos los roles]
  POST   /api/socios                   [admin, superadmin]
  PUT    /api/socios/{id}              [admin, superadmin]
  PUT    /api/socios/{id}/desactivar   [admin, superadmin]
  GET    /api/socios/estadisticas/total

BÚSQUEDA AVANZADA:
  Parámetros: search, estaActivo, page, pageSize
  Busca en: nombre, apellido, email, dni, numero_socio

AUTO-GENERACIÓN:
  El numero_socio se genera automáticamente:
    SOC-0001, SOC-0002, SOC-0003...


4.4. MÓDULO DE ACTIVIDADES
---------------------------
Archivo: ActividadService.cs

ENDPOINTS:
  GET    /api/actividades              [todos]
  GET    /api/actividades/{id}         [todos]
  POST   /api/actividades              [admin, superadmin]
  PUT    /api/actividades/{id}         [admin, superadmin]
  DELETE /api/actividades/{id}         [admin, superadmin]


4.5. MÓDULO DE MEMBRESÍAS
--------------------------
Archivo: MembresiaService.cs

ENDPOINTS:
  GET    /api/membresias               [todos]
  GET    /api/membresias/{id}          [todos]
  POST   /api/membresias               [admin, superadmin]
  PUT    /api/membresias/{id}          [admin, superadmin]
  DELETE /api/membresias/{id}          [admin, superadmin]
  POST   /api/membresias/asignar-actividad    [todos]
  POST   /api/membresias/remover-actividad    [admin, superadmin]
  GET    /api/membresias/estadisticas/total

FILTROS:
  - idSocio: Membresías de un socio específico
  - fechaDesde: Desde fecha
  - fechaHasta: Hasta fecha
  - soloImpagas: Solo con saldo > 0

WIZARD DE CREACIÓN (Frontend):
  Paso 1: Seleccionar socio (búsqueda)
  Paso 2: Definir fechas (inicio y fin)
  Paso 3: Seleccionar actividades
  → Calcula total automáticamente


4.6. MÓDULO DE PAGOS
---------------------
Archivo: PagoService.cs

ENDPOINTS:
  GET    /api/pagos                           [admin, superadmin]
  GET    /api/pagos/{id}                      [admin, superadmin]
  POST   /api/pagos                           [admin, superadmin]
  GET    /api/pagos/{id}/comprobante          [admin, superadmin]
  DELETE /api/pagos/{id}                      [admin, superadmin]
  GET    /api/pagos/metodos                   [todos]
  GET    /api/pagos/estadisticas              [admin, superadmin]
  GET    /api/pagos/estadisticas/recaudacion  [admin, superadmin]

WIZARD DE PAGO (Frontend):
  Paso 1: Seleccionar socio
  Paso 2: Seleccionar membresía impaga
  Paso 3: Ingresar monto y método de pago
  → Genera comprobante automáticamente

ESTADÍSTICAS:
  ✓ Total recaudado por período
  ✓ Pagos por método
  ✓ Pagos por día

================================================================================
5. CAMBIOS IMPORTANTES RECIENTES
================================================================================

5.1. MIGRACIÓN DE PERIODO A FECHAS (2025-11-28)
------------------------------------------------

ANTES:
  - periodo_anio: short (2025)
  - periodo_mes: byte (11)
  - Membresías solo mensuales
  - Validación de período duplicado

DESPUÉS:
  - fecha_inicio: date
  - fecha_fin: date
  - Membresías flexibles (cualquier rango)
  - Validación de solapamiento de fechas

IMPACTO:
  ✓ Backend: Entidades, DTOs, Services, Controllers actualizados
  ✓ Frontend: Formularios con date pickers en lugar de selectores
  ✓ Base de datos: Script de migración disponible
  ✓ Comprobantes: Muestran rango de fechas

SCRIPT DE MIGRACIÓN:
  Archivo: migration-membresias-fechas.sql
  Ejecutar ANTES de usar el sistema actualizado


5.2. ELIMINACIÓN DE CAMPO ES_CUOTA_BASE (Fecha anterior)
---------------------------------------------------------

ANTES:
  - Actividad.es_cuota_base: bool
  - Lógica especial para cuota base

DESPUÉS:
  - Todas las actividades son iguales
  - No hay concepto de cuota base
  - Simplificación del modelo

IMPACTO:
  ✓ Eliminado de entidad, DTOs, vistas


5.3. PERMISOS GRANULARES DE CREACIÓN DE USUARIOS (2025-11-28)
--------------------------------------------------------------

ANTES:
  - Solo superadmin podía crear usuarios

DESPUÉS:
  - Admin puede crear recepcionistas
  - Superadmin puede crear admin o recepcionistas
  - Validación en backend del rol del creador

IMPLEMENTACIÓN:
  - UsuarioService.CrearAsync recibe rolCreador
  - Controller pasa el rol del token JWT
  - Lanza UnauthorizedAccessException si no tiene permisos

================================================================================
6. REGLAS DE NEGOCIO
================================================================================

6.1. GESTIÓN DE SOCIOS
-----------------------
✓ Número de socio auto-generado (SOC-0001, SOC-0002...)
✓ Email y DNI únicos en el sistema
✓ Soft delete: fecha_eliminacion en lugar de borrado físico
✓ Desactivar socio marca fecha_baja

6.2. GESTIÓN DE MEMBRESÍAS
---------------------------
✓ fecha_fin debe ser posterior a fecha_inicio
✓ No se permiten fechas solapadas para el mismo socio
✓ Actividades con precio histórico (snapshot)
✓ Total = Suma de precios de actividades
✓ Saldo = Total - Pagos
✓ Modificar actividades solo si NO hay pagos
✓ Eliminar solo si NO hay pagos

6.3. GESTIÓN DE PAGOS
----------------------
✓ Monto > 0 y <= Saldo de la membresía
✓ Se permiten pagos parciales
✓ Generar comprobante automáticamente
✓ Usuario que procesa queda registrado
✓ Anular pago recalcula saldo

6.4. GESTIÓN DE USUARIOS
-------------------------
✓ Admin crea solo recepcionistas
✓ Superadmin crea admin o recepcionistas
✓ Nombre de usuario único
✓ Email único (compartido con Persona)
✓ Soft delete

6.5. GESTIÓN DE ACTIVIDADES
----------------------------
✓ Nombre único
✓ Precio no negativo
✓ Cambiar precio NO afecta membresías existentes
✓ Soft delete

================================================================================
7. PATRONES DE DISEÑO UTILIZADOS
================================================================================

7.1. SOFT DELETE PATTERN
-------------------------
Todas las entidades implementan soft delete:
  - fecha_eliminacion: datetime? (nullable)
  - Consultas filtran por fecha_eliminacion == null
  - Permite recuperación y auditoría

7.2. REPOSITORY PATTERN
------------------------
Aunque no hay repositorios explícitos, EF Core actúa como:
  - DbSet<T> = Repository
  - ClubDbContext = Unit of Work

7.3. SERVICE LAYER PATTERN
---------------------------
Lógica de negocio encapsulada en servicios:
  - IService → interface
  - Service → implementación
  - Inyectados vía DI

7.4. DTO PATTERN
----------------
Separación entre entidades de dominio y DTOs:
  - Entidades: Backend/Domain/Entities
  - DTOs: Backend/Application/DTOs
  - Mapeo manual en servicios

7.5. DEPENDENCY INJECTION
--------------------------
Todo configurado en Program.cs:
  - Services (Scoped)
  - DbContext (Scoped)
  - JWT Bearer (Singleton)

7.6. FLUENT API CONFIGURATION
------------------------------
Configuración de entidades en ClubDbContext:
  - HasColumnName
  - HasMaxLength
  - HasDefaultValueSql
  - Relationships con Fluent API

================================================================================
8. FLUJOS PRINCIPALES DEL SISTEMA
================================================================================

8.1. ALTA DE SOCIO
-------------------
1. Admin ingresa datos del socio
2. Frontend valida formato (nombre, DNI, email)
3. Backend valida unicidad (email, DNI)
4. Backend genera numero_socio (SOC-XXXX)
5. Crea Persona y Socio asociados
6. Retorna socio creado con número asignado

8.2. CREACIÓN DE MEMBRESÍA
---------------------------
1. Admin busca socio (por nombre/DNI/número)
2. Selecciona socio de resultados
3. Define fecha inicio y fecha fin
4. Sistema valida:
   - fecha_fin > fecha_inicio
   - Sin solapamiento con otras membresías del socio
5. Admin selecciona actividades
6. Sistema captura precio actual de cada actividad
7. Calcula total automáticamente
8. Crea membresía con actividades asociadas
9. Muestra resumen con total a pagar

8.3. REGISTRO DE PAGO
----------------------
1. Admin selecciona socio
2. Sistema muestra membresías impagas del socio
3. Admin selecciona membresía
4. Sistema muestra:
   - Total de la membresía
   - Total pagado anteriormente
   - Saldo pendiente
5. Admin ingresa:
   - Monto del pago
   - Método de pago (de lista en BD)
6. Sistema valida:
   - Monto > 0
   - Monto <= Saldo
7. Registra pago con:
   - Usuario que procesa
   - Fecha actual
8. Actualiza saldo de membresía
9. Genera comprobante (PAG-XXXXXX-YYYY)
10. Muestra comprobante en pantalla

8.4. GESTIÓN DE ACTIVIDADES EN MEMBRESÍA
-----------------------------------------
AGREGAR:
  1. Recepcionista/Admin selecciona membresía
  2. Selecciona actividad a agregar
  3. Sistema captura precio actual
  4. Agrega a membresia_actividades
  5. Recalcula total

REMOVER:
  1. Admin selecciona membresía
  2. Selecciona actividad a remover
  3. Sistema valida que NO haya pagos
  4. Remueve de membresia_actividades
  5. Recalcula total

8.5. CREACIÓN DE USUARIO
-------------------------
1. Admin/Superadmin ingresa datos
2. Selecciona rol según permisos:
   - Admin → solo "recepcionista"
   - Superadmin → "admin" o "recepcionista"
3. Sistema valida:
   - Permisos del creador
   - Unicidad de nombre_usuario
   - Unicidad de email
4. Crea Persona asociada
5. Crea Usuario con hash de contraseña
6. Retorna usuario creado

================================================================================
FIN DEL DOCUMENTO
================================================================================

Última actualización: 28 de noviembre de 2025
Versión del sistema: 1.2.0

Para más información, consultar:
  - CLAUDE.md: Guía de desarrollo
  - DiagramaClases.md: Diagrama UML actualizado
  - migration-membresias-fechas.sql: Script de migración de base de datos
