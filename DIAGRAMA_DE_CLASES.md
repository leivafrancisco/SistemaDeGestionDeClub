# Diagrama de Clases — Sistema de Gestión de Club

## 1. Entidades del Dominio (Domain Layer)

### Persona
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | Nombre | string |
| `+` | Apellido | string |
| `+` | Email | string |
| `+` | Dni | string? |
| `+` | FechaNacimiento | DateTime? |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | NombreCompleto *(propiedad calculada)* | string |

---

### Rol
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | Nombre | string |

---

### Usuario
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | IdPersona | int |
| `+` | IdRol | int |
| `+` | NombreUsuario | string |
| `+` | ContrasenaHash | string |
| `+` | EstaActivo | bool |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

---

### Socio
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | IdPersona | int |
| `+` | NumeroSocio | string |
| `+` | EstaActivo | bool |
| `+` | FechaAlta | DateTime |
| `+` | FechaBaja | DateTime? |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

---

### Actividad
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | Nombre | string |
| `+` | Descripcion | string? |
| `+` | Precio | decimal |
| `+` | EsCuotaBase | bool |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

---

### Membresia
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | IdSocio | int |
| `+` | FechaInicio | DateTime |
| `+` | FechaFin | DateTime |
| `+` | CostoTotal | decimal |
| `+` | Estado | string |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | TotalCargado *(calculado)* | decimal |
| `+` | TotalPagado *(calculado)* | decimal |
| `+` | Saldo *(calculado)* | decimal |

---

### MembresiaActividad *(tabla de unión)*
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | IdMembresia | int |
| `+` | IdActividad | int |
| `+` | PrecioAlMomento | decimal |

---

### Cuota
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | IdMembresia | int |
| `+` | NumeroCuota | int |
| `+` | Monto | decimal |
| `+` | FechaVencimiento | DateTime |
| `+` | Estado | string *(pendiente / pagada / vencida)* |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | EstaPagada *(calculado)* | bool |

---

### MetodoPago
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | Nombre | string |

---

### Pago
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | IdCuota | int |
| `+` | IdMetodoPago | int |
| `+` | IdUsuarioProcesa | int? |
| `+` | Monto | decimal |
| `+` | FechaPago | DateTime |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

---

### Asistencia
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | IdSocio | int |
| `+` | FechaHoraIngreso | DateTime |
| `+` | FechaCreacion | DateTime |
| `+` | FechaActualizacion | DateTime |
| `+` | FechaEliminacion | DateTime? |

---

### Auditoria
| Visibilidad | Atributo | Tipo |
|---|---|---|
| `+` | Id | int |
| `+` | Tabla | string |
| `+` | Operacion | string *(INSERT / UPDATE / DELETE)* |
| `+` | IdUsuario | int? |
| `+` | NombreUsuario | string? |
| `+` | FechaHora | DateTime |
| `+` | ValoresAnteriores | string? *(JSON)* |
| `+` | ValoresNuevos | string? *(JSON)* |
| `+` | NombreEntidad | string? |
| `+` | IdEntidad | string? |
| `+` | Detalles | string? |

---

## 2. Servicios (Application Layer)

### AuthService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | LoginAsync(LoginDto) | Task\<LoginResponseDto?\> |
| `+` | ObtenerUsuarioActualAsync(int userId) | Task\<UsuarioDto?\> |
| `+` | ActualizarPerfilAsync(int userId, ActualizarPerfilDto) | Task\<UsuarioDto?\> |
| `-` | GenerarTokenJwt(Usuario) | string |

---

### SocioService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | ObtenerTodosSociosAsync(search, estaActivo, page, pageSize) | Task\<List\<SocioDto\>\> |
| `+` | ObtenerSocioPorIdAsync(int id) | Task\<SocioDto?\> |
| `+` | ObtenerSocioPorNumeroAsync(string numeroSocio) | Task\<SocioDto?\> |
| `+` | CrearSocioAsync(CrearSocioDto) | Task\<SocioDto\> |
| `+` | ActualizarSocioAsync(int id, ActualizarSocioDto) | Task\<SocioDto\> |
| `+` | DesactivarSocioAsync(int id) | Task\<bool\> |
| `+` | ContarTotalSociosAsync() | Task\<int\> |

---

### MembresiaService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | ObtenerTodasMembresiasAsync(FiltrosMembresiasDto) | Task\<List\<MembresiaDto\>\> |
| `+` | ObtenerMembresiaPorIdAsync(int id) | Task\<MembresiaDto?\> |
| `+` | CrearMembresiaAsync(CrearMembresiaDto) | Task\<MembresiaDto\> |
| `+` | ActualizarMembresiaAsync(int id, ActualizarMembresiaDto) | Task\<MembresiaDto\> |
| `+` | EliminarMembresiaAsync(int id) | Task\<bool\> |
| `+` | ContarTotalMembresiasAsync() | Task\<int\> |
| `+` | AsignarActividadAsync(AsignarActividadDto) | Task\<MembresiaDto\> |
| `+` | RemoverActividadAsync(RemoverActividadDto) | Task\<MembresiaDto\> |
| `+` | ActualizarEstadoDespuesDePagoAsync(int idMembresia) | Task |
| `-` | GenerarCuotasAsync(Membresia) | Task\<List\<Cuota\>\> |
| `-` | CalcularMeses(DateTime inicio, DateTime fin) | int |
| `-` | MapearADto(Membresia) | MembresiaDto |

---

### CuotaService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | ObtenerCuotasAsync(FiltrosCuotasDto) | Task\<List\<CuotaDto\>\> |
| `+` | ObtenerCuotaPorIdAsync(int id) | Task\<CuotaDto?\> |
| `+` | ObtenerCuotasPorMembresiaAsync(int idMembresia) | Task\<List\<CuotaDto\>\> |
| `+` | ObtenerCuotasPorSocioAsync(int idSocio) | Task\<List\<CuotaDto\>\> |
| `+` | ObtenerMorososAsync() | Task\<List\<MorosoDto\>\> |
| `+` | ObtenerResumenAsync() | Task\<ResumenCuotasDto\> |
| `+` | GenerarCuotasParaMembresiaAsync(int idMembresia) | Task\<List\<CuotaDto\>\> |
| `+` | ActualizarEstadosVencidosAsync() | Task\<int\> |
| `+` | MarcarCuotaPagadaAsync(int idCuota) | Task |
| `+` | RevertirCuotaPagadaAsync(int idCuota) | Task |

---

### PagoService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | ObtenerTodosPagosAsync(FiltrosPagosDto) | Task\<List\<PagoDto\>\> |
| `+` | ObtenerPagoPorIdAsync(int id) | Task\<PagoDto?\> |
| `+` | RegistrarPagoAsync(RegistrarPagoDto, int idUsuario) | Task\<ComprobantePagoDto\> |
| `+` | GenerarComprobanteAsync(int idPago) | Task\<ComprobantePagoDto\> |
| `+` | AnularPagoAsync(int id) | Task\<bool\> |
| `+` | ObtenerTotalRecaudadoAsync(fechaDesde, fechaHasta) | Task\<decimal\> |
| `+` | ObtenerMetodosPagoAsync() | Task\<List\<MetodoPagoDto\>\> |
| `+` | ObtenerEstadisticasPagosAsync(fechaDesde, fechaHasta) | Task\<EstadisticasPagosDto\> |
| `-` | MapearADto(Pago) | PagoDto |

---

### ActividadService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | ObtenerTodasActividadesAsync() | Task\<List\<ActividadDto\>\> |
| `+` | ObtenerActividadPorIdAsync(int id) | Task\<ActividadDto?\> |
| `+` | CrearActividadAsync(CrearActividadDto) | Task\<ActividadDto\> |
| `+` | ActualizarActividadAsync(int id, ActualizarActividadDto) | Task\<ActividadDto\> |
| `+` | EliminarActividadAsync(int id) | Task\<bool\> |

---

### AsistenciaService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | VerificarEstadoSocioAsync(string dni) | Task\<VerificarAsistenciaDto\> |
| `+` | RegistrarAsistenciaAsync(string dni) | Task\<AsistenciaDto\> |
| `+` | ObtenerAsistenciasAsync(fecha, idSocio) | Task\<List\<AsistenciaDto\>\> |

---

### BackupService
| Visibilidad | Método | Retorno |
|---|---|---|
| `+` | CrearBackupAsync(BackupRequestDto) | Task\<BackupResponseDto\> |
| `+` | ObtenerBasesDatosDisponiblesAsync() | Task\<List\<string\>\> |
| `+` | ObtenerArchivoBackupAsync(string rutaCompleta) | Task\<(bool, byte[]?, string?, string?)\> |
| `+` | ObtenerBackupsDisponiblesAsync() | Task\<List\<BackupArchivoDto\>\> |
| `+` | RestaurarBackupAsync(RestoreRequestDto) | Task\<RestoreResponseDto\> |
| `-` | FormatearTamano(long bytes) | string |

---

## 3. Relaciones entre Clases

```
Persona ──────────────── 1 ──── 0..1 ── Usuario
Persona ──────────────── 1 ──── 0..1 ── Socio
Rol ───────────────────── 1 ──── * ───── Usuario
Socio ─────────────────── 1 ──── * ───── Membresia
Socio ─────────────────── 1 ──── * ───── Asistencia
Membresia ─────────────── 1 ──── * ───── MembresiaActividad
Actividad ─────────────── 1 ──── * ───── MembresiaActividad
Membresia ─────────────── 1 ──── * ───── Cuota
Cuota ─────────────────── 1 ──── * ───── Pago
MetodoPago ────────────── 1 ──── * ───── Pago
Usuario ───────────────── 1 ──── * ───── Pago (procesa)
```

---

## 4. Patrones de Diseño Detectados

### Creacionales

| Patrón | Ubicación | Descripción |
|---|---|---|
| **Singleton** | `ClubDbContext` registrado como `Scoped` en DI | Una sola instancia del contexto por request HTTP |
| **Método Fábrica** | `AuthService.GenerarTokenJwt()` | Crea y retorna un token JWT según el usuario recibido |

---

### Estructurales

| Patrón | Ubicación | Descripción |
|---|---|---|
| **Fachada** | `MembresiaService`, `PagoService` | Simplifican operaciones complejas (crear membresía genera cuotas y pago automáticamente) |
| **Adaptador** | `ClubDbContext` (EF Core) | Adapta las entidades del dominio al motor de base de datos SQL Server |
| **Repositorio** | `ClubDbContext` con `DbSet<T>` | Centraliza el acceso a datos, desacoplando el dominio de la persistencia |

---

### De Comportamiento

| Patrón | Ubicación | Descripción |
|---|---|---|
| **Estrategia** | `SqlServerRetryingExecutionStrategy` | Cambia el comportamiento de reintentos ante errores transitorios de DB |
| **Cadena de Responsabilidad** | Pipeline de middlewares ASP.NET (CORS → Auth → Authorization → Controllers) | Cada middleware decide si procesa o pasa la request al siguiente |
| **Estado** | `Cuota.Estado` y `Membresia.Estado` | El comportamiento del objeto varía según su estado *(pendiente / pagada / vencida / activa / pago_pendiente)* |
| **Observador** | `IAuditoriaService` | Registra cambios en el sistema de forma desacoplada después de cada operación |
| **Estrategia** | Interfaces `ISocioService`, `IMembresiaService`, etc. | Permiten intercambiar implementaciones concretas sin afectar a quien las consume — el controlador no conoce la implementación, solo el contrato |

---

## 5. Arquitectura General

```
┌─────────────────────────────────────────────┐
│                  API Layer                   │
│         Controllers (REST endpoints)         │
│  AuthController, SociosController, etc.      │
└────────────────────┬────────────────────────┘
                     │ usa interfaces
┌────────────────────▼────────────────────────┐
│              Application Layer               │
│    Services: Auth, Socio, Membresia,         │
│    Cuota, Pago, Actividad, Asistencia,       │
│    Backup, Auditoria                         │
└────────────────────┬────────────────────────┘
                     │ accede via EF Core
┌────────────────────▼────────────────────────┐
│             Infrastructure Layer             │
│    ClubDbContext (EF Core + SQL Server)      │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│               Domain Layer                   │
│    Entidades: Persona, Socio, Membresia,     │
│    Cuota, Pago, Actividad, etc.              │
└─────────────────────────────────────────────┘
```

---

*Arquitectura: Clean Architecture en capas — Domain → Infrastructure → Application → API*
