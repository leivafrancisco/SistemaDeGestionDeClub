# Conversaciones del Sistema — Club de Fútbol

Cada conversación muestra el diálogo, la llamada a la API y las funciones internas que ejecuta el sistema.

---

## Conversación 1 — Inicio de turno (Recepcionista)

**Contexto:** Lucía es recepcionista. Empieza su turno a las 8:00.

---

> **Lucía** abre el sistema e ingresa sus credenciales.

**Llamada API → HU-01**
```
POST /api/auth/login
{ "nombreUsuario": "recepcionista", "password": "recep123" }
```

**Funciones internas del sistema:**
```
AuthService.LoginAsync()
  1. Busca al usuario en la tabla usuarios por nombre_usuario
  2. Compara la contraseña recibida con contrasena_hash
  3. Si coincide, genera un token JWT con los claims:
       - NameIdentifier = usuario.Id
       - Name            = usuario.NombreUsuario
       - Email           = persona.Email
       - Role            = rol.Nombre
  4. El token expira en 24 horas
```

**Respuesta del sistema:**
```json
{
  "token": "eyJhbGci...",
  "nombreUsuario": "recepcionista",
  "rol": "recepcionista",
  "expira": "2026-04-16T08:00:00"
}
```

---

> **Lucía:** ¿Con qué usuario entré?

**Llamada API → HU-02**
```
GET /api/auth/me
Authorization: Bearer eyJhbGci...
```

**Funciones internas del sistema:**
```
AuthService.ObtenerUsuarioActualAsync()
  1. Lee el claim NameIdentifier del token JWT
  2. Busca en usuarios JOIN personas JOIN roles por ese ID
  3. Devuelve los datos del usuario autenticado
```

**Respuesta del sistema:**
```json
{
  "id": 2,
  "nombreUsuario": "recepcionista",
  "rol": "recepcionista",
  "email": "recepcionista@club.com"
}
```

---

## Conversación 2 — Socio llega al club (Recepcionista)

**Contexto:** Un socio se presenta en la puerta con su DNI.

---

> **Lucía:** Buenas, ¿su DNI?
> **Socio:** 38521047

**Llamada API → HU-29**
```
GET /api/asistencias/verificar/38521047
```

**Funciones internas del sistema:**
```
AsistenciaService.VerificarEstadoSocioAsync("38521047")
  1. Busca en socios JOIN personas WHERE dni = '38521047'
  2. Si no existe → lanza error "No se encontró un socio con DNI 38521047"
  3. Si EstaActivo = false → devuelve TieneAcceso = false, mensaje "Socio inactivo"
  4. Busca membresía vigente:
       WHERE id_socio = @id
         AND fecha_inicio <= NOW()
         AND fecha_fin    >= NOW()
         AND fecha_eliminacion IS NULL
       ORDER BY fecha_inicio DESC
  5. Si no hay membresía vigente → TieneAcceso = false, mensaje "Sin membresía vigente"
  6. Calcula Saldo = SUM(precio_mensual_congelado) - SUM(pagos activos)
  7. Si Saldo > 0 → TieneAcceso = false, mensaje "Tiene saldo pendiente de $X"
  8. Si todo OK → TieneAcceso = true
```

**Respuesta del sistema:**
```json
{
  "nombreSocio": "Carlos Rodríguez",
  "numeroSocio": "SOC-0042",
  "tieneAcceso": true,
  "mensaje": "✓ Socio con actividades al día. Acceso autorizado.",
  "estadoMembresia": "AL DIA",
  "actividades": ["Fútbol", "Gimnasio"],
  "fechaVigenciaHasta": "2026-05-31"
}
```

> **Lucía:** Todo bien, Carlos. Puede pasar.

**Llamada API → HU-30**
```
POST /api/asistencias/registrar/38521047
```

**Funciones internas del sistema:**
```
AsistenciaService.RegistrarAsistenciaAsync("38521047")
  1. Llama a VerificarEstadoSocioAsync() internamente
  2. Si TieneAcceso = false → lanza excepción con el mensaje de error
  3. Si tiene acceso:
       INSERT INTO asistencias (id_socio, fecha_hora_ingreso, ...)
       VALUES (@idSocio, NOW(), ...)
  4. ClubDbContext.SaveChangesAsync() persiste en la BD
  5. El trigger de auditoría registra el INSERT en la tabla auditoria
```

**Respuesta del sistema:**
```json
{
  "id": 512,
  "idSocio": 42,
  "numeroSocio": "SOC-0042",
  "nombreSocio": "Carlos Rodríguez",
  "fechaHoraIngreso": "2026-04-15T08:12:33"
}
```

---

## Conversación 3 — Socio con membresía vencida (Recepcionista)

**Contexto:** Un socio intenta ingresar pero su membresía está vencida.

---

> **Lucía:** ¿Su DNI por favor?
> **Socio:** 41882300

**Llamada API → HU-29**
```
GET /api/asistencias/verificar/41882300
```

**Funciones internas del sistema:**
```
AsistenciaService.VerificarEstadoSocioAsync("41882300")
  1. Encuentra al socio Martín López (EstaActivo = true)
  2. Busca membresía vigente con fecha_fin >= NOW()
  3. No encuentra ninguna (la última venció el 31/03/2026)
  4. Devuelve TieneAcceso = false
```

**Respuesta del sistema:**
```json
{
  "nombreSocio": "Martín López",
  "tieneAcceso": false,
  "mensaje": "No tiene membresía activa vigente. Por favor, renueve su membresía.",
  "estadoMembresia": "SIN MEMBRESIA VIGENTE"
}
```

> **Lucía:** Martín, su membresía venció. Tiene que pasar por administración para renovar.

---

## Conversación 4 — Nuevo socio (Admin)

**Contexto:** Andrés, el administrador, registra un nuevo socio.

---

> **Andrés:** Quiero registrar a un nuevo socio.

**Llamada API → HU-05**
```
POST /api/socios
{
  "nombre": "Federico",
  "apellido": "Gómez",
  "email": "fede.gomez@gmail.com",
  "dni": "44123789",
  "fechaNacimiento": "2000-06-15"
}
```

**Funciones internas del sistema:**
```
SocioService.CrearAsync()
  1. Valida email único:
       SELECT COUNT(*) FROM personas
       WHERE email = 'fede.gomez@gmail.com' AND fecha_eliminacion IS NULL
       → Si > 0: lanza "Ya existe un socio con este email"

  2. Valida DNI único:
       SELECT COUNT(*) FROM personas
       WHERE dni = '44123789' AND fecha_eliminacion IS NULL
       → Si > 0: lanza "Ya existe un socio con este DNI"

  3. Genera número de socio automáticamente:
       SELECT TOP 1 numero_socio FROM socios
       WHERE numero_socio LIKE 'SOC-%'
       ORDER BY numero_socio DESC
       → Extrae el número, suma 1 → "SOC-0087"

  4. INSERT INTO personas (nombre, apellido, email, dni, fecha_nacimiento, ...)
  5. INSERT INTO socios (id_persona, numero_socio, esta_activo, fecha_alta, ...)
  6. ClubDbContext.SaveChangesAsync() × 2
  7. Auditoría registra 2 INSERTs (personas + socios)
```

**Respuesta del sistema:**
```json
{
  "id": 87,
  "numeroSocio": "SOC-0087",
  "nombre": "Federico",
  "apellido": "Gómez",
  "estaActivo": true,
  "fechaAlta": "2026-04-15"
}
```

---

> **Andrés:** Ahora le creo la membresía. Primero veo qué actividades hay.

**Llamada API → HU-09**
```
GET /api/actividades
```

**Funciones internas del sistema:**
```
ActividadService.ObtenerTodasAsync()
  1. SELECT * FROM actividades WHERE fecha_eliminacion IS NULL
  2. Solo devuelve actividades activas
```

**Respuesta del sistema:**
```json
[
  { "id": 1, "nombre": "Fútbol",   "precio": 8000.00 },
  { "id": 2, "nombre": "Gimnasio", "precio": 6000.00 },
  { "id": 3, "nombre": "Natación", "precio": 7000.00 }
]
```

> **Federico eligió Fútbol y Gimnasio por 1 mes.**

**Llamada API → HU-15**
```
POST /api/membresias
{
  "idSocio": 87,
  "fechaInicio": "2026-04-15",
  "fechaFin": "2026-05-14",
  "idsActividades": [1, 2],
  "costoTotal": 14000.00,
  "monto": 14000.00,
  "idMetodoPago": 1
}
```

**Funciones internas del sistema:**
```
MembresiaService.CrearAsync()
  1. Valida que el socio existe (id = 87, fecha_eliminacion IS NULL)
  2. Valida fechas: FechaFin (2026-05-14) > FechaInicio (2026-04-15) ✓
  3. Valida solapamiento de fechas:
       SELECT COUNT(*) FROM membresias
       WHERE id_socio = 87
         AND fecha_eliminacion IS NULL
         AND (fecha_inicio <= '2026-05-14' AND fecha_fin >= '2026-04-15')
       → 0, no hay solapamiento ✓
  4. Valida que las actividades [1, 2] existan y no estén eliminadas
  5. Valida costoTotal > 0 y monto > 0
  6. Valida que el método de pago (id=1, Efectivo) existe
  7. INSERT INTO membresias (id_socio, fecha_inicio, fecha_fin, costo_total, estado, ...)
  8. Para cada actividad:
       INSERT INTO membresia_actividades (id_membresia, id_actividad, precio_mensual_congelado)
       VALUES (201, 1, 8000.00)   -- Fútbol: precio congelado al momento
       VALUES (201, 2, 6000.00)   -- Gimnasio: precio congelado al momento
  9. INSERT INTO pagos (id_membresia, id_metodo_pago, monto, fecha_pago, ...)
  10. ClubDbContext.SaveChangesAsync() × 3
  11. Auditoría registra INSERTs en membresias, membresia_actividades y pagos
```

**Respuesta del sistema:**
```json
{
  "id": 201,
  "numeroSocio": "SOC-0087",
  "fechaInicio": "2026-04-15",
  "fechaFin": "2026-05-14",
  "actividades": [
    { "nombre": "Fútbol",   "precioAlMomento": 8000.00 },
    { "nombre": "Gimnasio", "precioAlMomento": 6000.00 }
  ],
  "costoTotal": 14000.00,
  "totalPagado": 14000.00,
  "saldo": 0.00,
  "estadoPago": "PAGADO"
}
```

---

## Conversación 5 — Cobro de membresía pendiente (Admin)

**Contexto:** Un socio viene a pagar su membresía que quedó pendiente.

---

> **Andrés:** ¿Qué métodos de pago están disponibles?

**Llamada API → HU-26**
```
GET /api/pagos/metodos
```

**Funciones internas del sistema:**
```
PagoService.ObtenerMetodosPagoAsync()
  1. SELECT * FROM metodos_pago
  2. Devuelve todos los métodos sin filtro
```

**Respuesta del sistema:**
```json
[
  { "id": 1, "nombre": "Efectivo" },
  { "id": 2, "nombre": "Tarjeta" },
  { "id": 3, "nombre": "Transferencia" }
]
```

> **Socio** entrega $14.000 en efectivo.

**Llamada API → HU-23**
```
POST /api/pagos
{ "idMembresia": 205, "idMetodoPago": 1, "monto": 14000.00 }
```

**Funciones internas del sistema:**
```
PagoService.RegistrarPagoAsync()
  1. Busca la membresía 205 con JOIN a socio, persona, actividades y pagos anteriores
  2. Valida que el método de pago (id=1) existe
  3. Valida monto > 0
  4. Calcula saldo actual:
       totalCargado = SUM(precio_mensual_congelado) = 14000.00
       totalPagado  = SUM(pagos activos)            = 0.00
       saldoActual  = 14000.00 - 0.00 = 14000.00
  5. Valida que monto (14000) no supere saldoActual (14000) ✓
  6. INSERT INTO pagos (id_membresia, id_metodo_pago, id_usuario_procesa, monto, fecha_pago)
  7. ClubDbContext.SaveChangesAsync()
  8. Llama a MembresiaService.ActualizarEstadoDespuesDePagoAsync(205):
       → Recalcula saldo = 14000 - 14000 = 0
       → Estado = Activa
  9. Llama a GenerarComprobanteAsync() para armar el comprobante:
       - Número: PAG-000312-2026
       - Calcula saldoAntes = totalCargado - totalPagadoSinEstePago
       - Calcula saldoDespues = totalCargado - totalPagadoConEstePago
  10. Auditoría registra INSERT en pagos y UPDATE en membresias
```

**Respuesta del sistema:**
```json
{
  "numeroPago": "PAG-000312-2026",
  "nombreSocio": "Federico Gómez",
  "actividades": ["Fútbol", "Gimnasio"],
  "metodoPago": "Efectivo",
  "monto": 14000.00,
  "saldoAntes": 14000.00,
  "saldoDespues": 0.00,
  "estaPaga": true
}
```

> **Andrés:** Listo Federico, su membresía está paga. Comprobante: PAG-000312-2026.

---

## Conversación 6 — Pago parcial en dos cuotas (Admin)

**Contexto:** Una socia no tiene para pagar todo, abona la mitad.

---

> **Socia:** Solo tengo $7.000 hoy, el resto lo traigo la semana que viene.

**Llamada API → HU-23**
```
POST /api/pagos
{ "idMembresia": 198, "idMetodoPago": 1, "monto": 7000.00 }
```

**Funciones internas del sistema:**
```
PagoService.RegistrarPagoAsync()
  1. Calcula saldo: totalCargado=14000, totalPagado=0, saldoActual=14000
  2. Valida 7000 <= 14000 ✓
  3. INSERT INTO pagos (monto=7000)
  4. MembresiaService.ActualizarEstadoDespuesDePagoAsync(198):
       saldo = 14000 - 7000 = 7000 → Estado = PagoPendiente
```

**Respuesta del sistema:**
```json
{ "monto": 7000.00, "saldoAntes": 14000.00, "saldoDespues": 7000.00, "estaPaga": false }
```

> **Andrés:** Registrado. Le queda saldo de $7.000.

*(La semana siguiente)*

**Llamada API → HU-23**
```
POST /api/pagos
{ "idMembresia": 198, "idMetodoPago": 3, "monto": 7000.00 }
```

**Funciones internas del sistema:**
```
PagoService.RegistrarPagoAsync()
  1. Calcula saldo: totalCargado=14000, totalPagado=7000, saldoActual=7000
  2. Valida 7000 <= 7000 ✓
  3. INSERT INTO pagos (monto=7000, metodo=Transferencia)
  4. MembresiaService.ActualizarEstadoDespuesDePagoAsync(198):
       saldo = 14000 - 14000 = 0 → Estado = Activa
```

**Respuesta del sistema:**
```json
{ "monto": 7000.00, "saldoAntes": 7000.00, "saldoDespues": 0.00, "estaPaga": true }
```

---

## Conversación 7 — Agregar actividad a membresía vigente (Recepcionista)

**Contexto:** Un socio quiere sumar Natación a su membresía actual.

---

> **Socio:** Quiero anotarme en Natación también.

**Llamada API → HU-04**
```
GET /api/socios/numero/SOC-0042
```

**Funciones internas del sistema:**
```
SocioService.ObtenerPorNumeroSocioAsync("SOC-0042")
  1. SELECT socios JOIN personas
     WHERE numero_socio = 'SOC-0042' AND fecha_eliminacion IS NULL
```

**Llamada API → HU-18**
```
POST /api/membresias/asignar-actividad
{ "idMembresia": 195, "idActividad": 3 }
```

**Funciones internas del sistema:**
```
MembresiaService.AsignarActividadAsync()
  1. Busca membresía 195 con sus actividades actuales
  2. Busca actividad 3 (Natación, precio=7000) WHERE fecha_eliminacion IS NULL
  3. Verifica que Natación no esté ya asignada:
       membresiaActividades.Any(ma => ma.IdActividad == 3) → false ✓
  4. Congela el precio actual:
       INSERT INTO membresia_actividades
       (id_membresia=195, id_actividad=3, precio_mensual_congelado=7000.00)
  5. UPDATE membresias SET fecha_actualizacion = NOW() WHERE id = 195
  6. ClubDbContext.SaveChangesAsync()
  7. Recarga la membresía con todas sus relaciones
  8. Recalcula en el DTO:
       TotalCargado = 8000 + 6000 + 7000 = 21000
       Saldo = 21000 - 14000 (ya pagado) = 7000
```

**Respuesta del sistema:**
```json
{
  "actividades": ["Fútbol", "Gimnasio", "Natación"],
  "costoTotal": 21000.00,
  "totalPagado": 14000.00,
  "saldo": 7000.00
}
```

> **Lucía:** Listo Carlos, quedó anotado en Natación. Le queda un saldo de $7.000.

---

## Conversación 8 — Cierre de caja (Admin)

**Contexto:** Al final del día, Andrés revisa la recaudación.

---

> **Andrés:** ¿Cuánto recaudamos hoy?

**Llamada API → HU-28**
```
GET /api/pagos/estadisticas/recaudacion?fechaDesde=2026-04-15&fechaHasta=2026-04-15
```

**Funciones internas del sistema:**
```
PagoService.ObtenerTotalRecaudadoAsync()
  1. SELECT SUM(monto) FROM pagos
     WHERE fecha_eliminacion IS NULL
       AND fecha_pago >= '2026-04-15'
       AND fecha_pago <= '2026-04-15'
```

**Respuesta del sistema:**
```json
{ "total": 35000.00, "fechaDesde": "2026-04-15", "fechaHasta": "2026-04-15" }
```

> **Andrés:** $35.000 hoy. ¿Cuál fue el desglose?

**Llamada API → HU-27**
```
GET /api/pagos/estadisticas?fechaDesde=2026-04-15&fechaHasta=2026-04-15
```

**Funciones internas del sistema:**
```
PagoService.ObtenerEstadisticasAsync()
  1. Trae todos los pagos del período con JOIN a metodos_pago
  2. Calcula totalRecaudado = SUM(monto) del período
  3. Calcula totalPagosHoy:
       SELECT SUM(monto) WHERE fecha_pago = TODAY()
  4. Calcula totalPagosMes:
       SELECT SUM(monto) WHERE fecha_pago BETWEEN primer_dia_mes AND ultimo_dia_mes
  5. Calcula totalPendiente:
       Para cada membresía activa:
         saldo = SUM(precio_congelado) - SUM(pagos activos)
         suma los saldos positivos
  6. Agrupa por método de pago: GroupBy(metodo).Sum(monto)
  7. Agrupa por día: GroupBy(fecha).Sum(monto)
```

**Respuesta del sistema:**
```json
{
  "totalRecaudado": 35000.00,
  "cantidadPagos": 4,
  "porMetodo": [
    { "metodo": "Efectivo",      "total": 21000.00, "cantidad": 3 },
    { "metodo": "Transferencia", "total": 14000.00, "cantidad": 1 }
  ]
}
```

---

## Conversación 9 — Anular un pago por error (Admin)

**Contexto:** Andrés registró un pago en la membresía equivocada.

---

> **Andrés:** Me equivoqué de membresía, necesito anular el pago 312.

**Llamada API → HU-25**
```
DELETE /api/pagos/312
```

**Funciones internas del sistema:**
```
PagoService.AnularPagoAsync(312)
  1. Busca el pago: SELECT * FROM pagos WHERE id = 312
  2. Verifica que no esté ya anulado (fecha_eliminacion IS NULL)
  3. Soft delete:
       UPDATE pagos SET fecha_eliminacion = NOW() WHERE id = 312
  4. ClubDbContext.SaveChangesAsync()
  5. Auditoría registra UPDATE en pagos (fecha_eliminacion: null → NOW())
  6. NOTA: el saldo de la membresía se recalcula automáticamente en el próximo GET
       porque Saldo = SUM(precios) - SUM(pagos WHERE fecha_eliminacion IS NULL)
```

**Respuesta del sistema:**
```json
{ "message": "Pago anulado exitosamente" }
```

> **Andrés:** Ahora lo registro en la membresía correcta.

---

## Conversación 10 — Historial de asistencias (Recepcionista)

**Contexto:** Lucía quiere ver quién concurrió hoy.

---

> **Lucía:** ¿Quiénes vinieron hoy?

**Llamada API → HU-31**
```
GET /api/asistencias?fecha=2026-04-15
```

**Funciones internas del sistema:**
```
AsistenciaService.ObtenerAsistenciasAsync(fecha="2026-04-15")
  1. Parsea la fecha a DateTime con formato "yyyy-MM-dd"
  2. Calcula rango: fechaInicio = 2026-04-15 00:00:00, fechaFin = 2026-04-16 00:00:00
  3. SELECT asistencias JOIN socios JOIN personas
     WHERE fecha_eliminacion IS NULL
       AND fecha_hora_ingreso >= '2026-04-15 00:00:00'
       AND fecha_hora_ingreso <  '2026-04-16 00:00:00'
     ORDER BY fecha_hora_ingreso DESC
```

**Respuesta del sistema:**
```json
[
  { "nombreSocio": "Carlos Rodríguez", "fechaHoraIngreso": "2026-04-15T08:12:33" },
  { "nombreSocio": "Ana Fernández",    "fechaHoraIngreso": "2026-04-15T09:45:10" },
  { "nombreSocio": "Luis Herrera",     "fechaHoraIngreso": "2026-04-15T11:03:55" }
]
```

> **Lucía:** 3 socios hasta ahora.

---

## Conversación 11 — Dar de baja a un socio (Admin)

**Contexto:** Un socio comunica que deja el club.

---

> **Andrés:** El socio SOC-0023 quiere darse de baja.

**Llamada API → HU-07**
```
PUT /api/socios/23/desactivar
```

**Funciones internas del sistema:**
```
SocioService.DesactivarAsync(23)
  1. Busca el socio: SELECT * FROM socios WHERE id = 23
  2. Verifica que exista y no esté ya eliminado (fecha_eliminacion IS NULL)
  3. Soft delete parcial (no elimina, solo desactiva):
       UPDATE socios
       SET esta_activo = false,
           fecha_baja  = NOW()
       WHERE id = 23
  4. ClubDbContext.SaveChangesAsync()
  5. NOTA: fecha_eliminacion permanece NULL → el historial (membresías, pagos,
     asistencias) queda accesible. Solo se bloquea el acceso físico al club.
  6. Auditoría registra UPDATE en socios
```

**Respuesta del sistema:**
```json
{ "message": "Socio desactivado exitosamente" }
```

---

## Conversación 12 — Crear usuario recepcionista (Admin)

**Contexto:** Contratan a una nueva recepcionista y Andrés le crea su acceso.

---

> **Andrés:** Necesito crear acceso para la nueva recepcionista, María.

**Llamada API → HU-34**
```
POST /api/usuarios
{
  "nombre": "María", "apellido": "Torres",
  "email": "maria.torres@club.com",
  "nombreUsuario": "maria_torres",
  "password": "recep456",
  "idRol": 3
}
Authorization: Bearer <token-de-admin>
```

**Funciones internas del sistema:**
```
UsuarioService.CrearAsync(dto, rolCreador="admin")
  1. Lee el rol del token del admin (claim Role = "admin")
  2. Verifica permisos:
       Si rolCreador = "admin" → solo puede crear rol "recepcionista" (id=3)
       Si intenta crear "admin" o "superadmin" → lanza UnauthorizedAccessException
  3. Valida nombre de usuario único:
       SELECT COUNT(*) FROM usuarios WHERE nombre_usuario = 'maria_torres'
  4. Valida email único:
       SELECT COUNT(*) FROM personas WHERE email = 'maria.torres@club.com'
  5. INSERT INTO personas (nombre, apellido, email, ...)
  6. INSERT INTO usuarios (id_persona, id_rol=3, nombre_usuario, contrasena_hash, ...)
  7. ClubDbContext.SaveChangesAsync() × 2
  8. Auditoría registra INSERTs en personas y usuarios
```

**Respuesta del sistema:**
```json
{
  "id": 5,
  "nombreUsuario": "maria_torres",
  "rol": "recepcionista",
  "estaActivo": true
}
```

---

## Conversación 13 — Buscar socios con deuda (Admin)

**Contexto:** Andrés quiere saber quién tiene saldo pendiente.

---

> **Andrés:** ¿Qué membresías están impagas?

**Llamada API → HU-13**
```
GET /api/membresias?soloImpagas=true
```

**Funciones internas del sistema:**
```
MembresiaService.ObtenerTodosAsync(filtros.SoloImpagas=true)
  1. SELECT membresias JOIN socios JOIN personas JOIN membresia_actividades JOIN pagos
     WHERE fecha_eliminacion IS NULL
     ORDER BY fecha_inicio DESC
  2. Para cada membresía calcula en MapearADto():
       TotalCargado = SUM(precio_mensual_congelado)
       TotalPagado  = SUM(pagos WHERE fecha_eliminacion IS NULL)
       Saldo        = TotalCargado - TotalPagado
       EstaPaga     = TotalCargado <= TotalPagado
  3. Filtra DESPUÉS de mapear:
       membresiasDtos.Where(m => !m.EstaPaga)
       (el filtro SoloImpagas se aplica en memoria, no en SQL)
```

**Respuesta del sistema:**
```json
[
  { "numeroSocio": "SOC-0017", "nombreSocio": "Martín López",  "saldo": 15000.00 },
  { "numeroSocio": "SOC-0031", "nombreSocio": "Paula Sánchez", "saldo": 7000.00  },
  { "numeroSocio": "SOC-0056", "nombreSocio": "Roberto Díaz",  "saldo": 14000.00 }
]
```

> **Andrés:** Tres socios con saldo pendiente. Los voy a contactar.

---

## Conversación 14 — Revisar auditoría (Superadmin)

**Contexto:** El superadmin quiere verificar quién anuló un pago.

---

> **Superadmin:** ¿Quién anuló el pago ID 312 y a qué hora?

**Llamada API → HU-37**
```
GET /api/auditoria?tabla=pagos&operacion=UPDATE
Authorization: Bearer <token-de-superadmin>
```

**Funciones internas del sistema:**
```
AuditoriaService.ObtenerAuditoriasAsync()
  1. SELECT * FROM auditoria
     WHERE tabla = 'pagos' AND operacion = 'UPDATE'
     ORDER BY fecha_hora DESC
  2. Devuelve nombre_usuario, fecha_hora, valores_anteriores (JSON), valores_nuevos (JSON)

  El registro fue creado automáticamente por ClubDbContext.SaveChangesAsync()
  cuando AnularPagoAsync() hizo el soft delete:
    - valores_anteriores: { "fechaEliminacion": null }
    - valores_nuevos:     { "fechaEliminacion": "2026-04-15T16:42:11" }
```

**Respuesta del sistema:**
```json
{
  "tabla": "pagos",
  "operacion": "UPDATE",
  "nombreUsuario": "admin",
  "fechaHora": "2026-04-15T16:42:11",
  "detalles": "Modificó Pago (ID: 312)",
  "valoresAnteriores": { "fechaEliminacion": null },
  "valoresNuevos": { "fechaEliminacion": "2026-04-15T16:42:11" }
}
```

> **Superadmin:** Fue Andrés (admin) a las 16:42. Todo en orden.

---

## Conversación 15 — Renovar membresía de socio existente con seña (Admin)

**Contexto:** Un socio con membresía vencida viene a renovar pero solo puede abonar una parte hoy.

---

> **Andrés:** Busco al socio con DNI 38521047.

**Llamada API → HU-03**
```
GET /api/socios?search=38521047
```

**Funciones internas del sistema:**
```
SocioService.ObtenerTodosAsync(search="38521047")
  1. SELECT socios JOIN personas
     WHERE fecha_eliminacion IS NULL
       AND (dni = '38521047' OR email LIKE '%38521047%' OR ...)
```

**Respuesta del sistema:**
```json
{
  "id": 42,
  "numeroSocio": "SOC-0042",
  "nombre": "Carlos",
  "apellido": "Rodríguez",
  "estaActivo": true
}
```

> **Andrés:** ¿Qué actividades tiene disponibles?

**Llamada API → HU-09**
```
GET /api/actividades
```

**Funciones internas del sistema:**
```
ActividadService.ObtenerTodasAsync()
  1. SELECT * FROM actividades WHERE fecha_eliminacion IS NULL
  NOTA: es_cuota_base existe en la BD pero el sistema no lo usa.
        Todas las actividades se tratan igual.
```

**Respuesta del sistema:**
```json
[
  { "id": 1, "nombre": "Fútbol",   "precio": 8000.00 },
  { "id": 2, "nombre": "Gimnasio", "precio": 6000.00 },
  { "id": 3, "nombre": "Natación", "precio": 7000.00 }
]
```

> **Carlos:** Quiero Fútbol y Gimnasio por un mes. Hoy dejo $10.000 de seña y el resto la semana que viene.

**Llamada API → HU-15**
```
POST /api/membresias
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

**Funciones internas del sistema:**
```
MembresiaService.CrearAsync()
  1. Valida que el socio 42 existe y está activo
  2. Valida fechas: FechaFin (2026-05-15) > FechaInicio (2026-04-16) ✓
  3. Valida solapamiento:
       SELECT COUNT(*) FROM membresias
       WHERE id_socio = 42
         AND fecha_eliminacion IS NULL
         AND (fecha_inicio <= '2026-05-15' AND fecha_fin >= '2026-04-16')
       → 0, no hay solapamiento ✓
  4. Valida costoTotal > 0 ✓ y monto > 0 ✓
     NOTA: monto (10000) puede ser menor a costoTotal (14000) — pago parcial
  5. Valida que el método de pago (id=1, Efectivo) existe
  6. INSERT INTO membresias (id_socio=42, fecha_inicio, fecha_fin, costo_total=14000)
  7. Congela precios actuales:
       INSERT INTO membresia_actividades (id_membresia, id_actividad=1, precio_mensual_congelado=8000)
       INSERT INTO membresia_actividades (id_membresia, id_actividad=2, precio_mensual_congelado=6000)
  8. Registra pago inicial (solo la seña):
       INSERT INTO pagos (id_membresia, id_metodo_pago=1, monto=10000, fecha_pago=NOW())
  9. ClubDbContext.SaveChangesAsync() × 3
  10. Calcula en el DTO:
       TotalCargado = 8000 + 6000 = 14000
       TotalPagado  = 10000
       Saldo        = 14000 - 10000 = 4000  ← queda deuda
       EstaPaga     = false
```

**Respuesta del sistema:**
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

> **Andrés:** Listo Carlos, membresía creada. Le queda un saldo de $4.000 para la semana que viene.

*(La semana siguiente, Carlos trae el resto)*

**Llamada API → HU-23**
```
POST /api/pagos
{ "idMembresia": 210, "idMetodoPago": 1, "monto": 4000.00 }
```

**Funciones internas del sistema:**
```
PagoService.RegistrarPagoAsync()
  1. Calcula saldo: totalCargado=14000, totalPagado=10000, saldoActual=4000
  2. Valida 4000 <= 4000 ✓
  3. INSERT INTO pagos (monto=4000)
  4. MembresiaService.ActualizarEstadoDespuesDePagoAsync(210):
       saldo = 14000 - 14000 = 0 → Estado = Activa
```

**Respuesta del sistema:**
```json
{
  "numeroPago": "PAG-000315-2026",
  "nombreSocio": "Carlos Rodríguez",
  "monto": 4000.00,
  "saldoAntes": 4000.00,
  "saldoDespues": 0.00,
  "estaPaga": true
}
```

> **Andrés:** Listo Carlos, membresía totalmente paga. Comprobante: PAG-000315-2026.
