# Historias de Usuario — Sistema de Gestión de Club de Fútbol

## Roles del sistema

| Rol | Descripción |
|---|---|
| **superadmin** | Acceso total al sistema. Gestión de usuarios y configuración general. |
| **admin** | Gestión de socios, membresías, pagos y reportes. |
| **recepcionista** | Consulta de socios, registro de asistencias y asignación de actividades. |

---

## Autenticación

### HU-01 — Iniciar sesión
**Como** cualquier usuario del sistema  
**Quiero** ingresar con mi nombre de usuario y contraseña  
**Para** acceder a las funcionalidades según mi rol

**Criterios de aceptación:**
- El sistema valida usuario y contraseña
- Si las credenciales son correctas, devuelve un token JWT válido por 24 horas
- Si las credenciales son incorrectas, devuelve un mensaje de error
- El token incluye: ID, nombre de usuario, email y rol del usuario

---

### HU-02 — Ver mi perfil
**Como** usuario autenticado  
**Quiero** consultar mis datos de sesión actuales  
**Para** verificar con qué usuario estoy operando y qué permisos tengo

---

## Gestión de Socios

### HU-03 — Listar socios
**Como** usuario autenticado (cualquier rol)  
**Quiero** ver el listado de socios del club  
**Para** consultar su información y estado

**Criterios de aceptación:**
- Permite buscar por nombre, apellido, email, DNI o número de socio
- Permite filtrar por estado activo/inactivo
- Soporta paginación
- Muestra número de socio, datos personales y estado

---

### HU-04 — Ver detalle de un socio
**Como** usuario autenticado  
**Quiero** consultar todos los datos de un socio específico  
**Para** ver su información completa

**Criterios de aceptación:**
- Se puede buscar por ID o por número de socio (ej: SOC-0001)
- Muestra datos personales, fecha de alta y estado

---

### HU-05 — Dar de alta un socio
**Como** admin o superadmin  
**Quiero** registrar un nuevo socio en el sistema  
**Para** incorporarlo al club

**Criterios de aceptación:**
- Requiere nombre, apellido y email
- El email y el DNI deben ser únicos en el sistema
- El número de socio se genera automáticamente (formato SOC-0001, SOC-0002, etc.)
- El socio queda activo por defecto

---

### HU-06 — Editar datos de un socio
**Como** admin o superadmin  
**Quiero** modificar los datos personales de un socio  
**Para** mantener la información actualizada

**Criterios de aceptación:**
- Permite actualizar nombre, apellido, email, DNI y fecha de nacimiento
- El email y DNI no pueden duplicarse con otros socios

---

### HU-07 — Dar de baja a un socio
**Como** admin o superadmin  
**Quiero** desactivar a un socio del club  
**Para** registrar su baja sin eliminar su historial

**Criterios de aceptación:**
- El socio queda marcado como inactivo
- Se registra la fecha de baja
- Los datos históricos (membresías, pagos, asistencias) se conservan

---

### HU-08 — Ver total de socios activos
**Como** usuario autenticado  
**Quiero** ver la cantidad total de socios activos  
**Para** tener una estadística rápida del club

---

## Gestión de Actividades

### HU-09 — Listar actividades disponibles
**Como** usuario autenticado  
**Quiero** ver todas las actividades del club con sus precios  
**Para** conocer la oferta disponible

**Criterios de aceptación:**
- Solo muestra actividades activas (no eliminadas)
- Muestra nombre, descripción y precio actual

---

### HU-10 — Crear una actividad
**Como** admin o superadmin  
**Quiero** agregar una nueva actividad al catálogo  
**Para** ofrecerla a los socios en sus membresías

**Criterios de aceptación:**
- Requiere nombre y precio
- El nombre debe ser único
- La descripción es opcional

---

### HU-11 — Editar una actividad
**Como** admin o superadmin  
**Quiero** modificar el nombre, descripción o precio de una actividad  
**Para** mantener el catálogo actualizado

**Criterios de aceptación:**
- El cambio de precio no afecta membresías ya creadas (precio congelado al momento de alta)

---

### HU-12 — Eliminar una actividad
**Como** admin o superadmin  
**Quiero** dar de baja una actividad del catálogo  
**Para** que no pueda ser asignada en nuevas membresías

**Criterios de aceptación:**
- La eliminación es lógica (soft delete)
- No se puede eliminar si está en uso en membresías activas

---

## Gestión de Membresías

### HU-13 — Listar membresías
**Como** usuario autenticado  
**Quiero** ver las membresías registradas en el sistema  
**Para** consultar su estado y detalles

**Criterios de aceptación:**
- Permite filtrar por socio, rango de fechas, estado de pago y vigencia
- Muestra el costo total, lo pagado y el saldo pendiente
- Soporta paginación

---

### HU-14 — Ver detalle de una membresía
**Como** usuario autenticado  
**Quiero** consultar todos los datos de una membresía  
**Para** ver sus actividades, pagos y saldo

---

### HU-15 — Crear una membresía
**Como** admin o superadmin  
**Quiero** crear una membresía para un socio  
**Para** inscribirlo en actividades por un período determinado

**Criterios de aceptación:**
- Requiere socio, fecha de inicio, fecha de fin y al menos una actividad
- La fecha de fin debe ser posterior a la fecha de inicio
- No puede haber membresías con rangos de fechas superpuestos para el mismo socio
- El costo total se calcula automáticamente según las actividades seleccionadas y la duración
- El precio de cada actividad queda congelado al momento de la creación

---

### HU-16 — Editar una membresía
**Como** admin o superadmin  
**Quiero** modificar las fechas y/o actividades de una membresía  
**Para** corregir o actualizar su información

**Criterios de aceptación:**
- Se pueden cambiar las fechas de inicio y fin
- Se pueden agregar o quitar actividades
- Los pagos existentes se mantienen y el saldo se recalcula automáticamente
- El saldo puede quedar negativo si hay saldo a favor del socio

---

### HU-17 — Eliminar una membresía
**Como** admin o superadmin  
**Quiero** eliminar una membresía del sistema  
**Para** corregir altas erróneas

**Criterios de aceptación:**
- La eliminación es lógica (soft delete)

---

### HU-18 — Asignar actividad a una membresía
**Como** recepcionista, admin o superadmin  
**Quiero** agregar una actividad a una membresía existente  
**Para** inscribir al socio en una nueva actividad

**Criterios de aceptación:**
- La actividad no puede estar ya asignada a esa membresía
- El precio se congela al momento de la asignación

---

### HU-19 — Quitar actividad de una membresía
**Como** recepcionista, admin o superadmin  
**Quiero** remover una actividad de una membresía  
**Para** corregir asignaciones erróneas

**Criterios de aceptación:**
- Solo se permite si la membresía no tiene pagos registrados
- Protege la integridad de los comprobantes de pago existentes

---

### HU-20 — Ver total de membresías
**Como** usuario autenticado  
**Quiero** ver la cantidad total de membresías registradas  
**Para** tener una estadística del sistema

---

## Gestión de Pagos

### HU-21 — Listar pagos
**Como** usuario autenticado  
**Quiero** ver el historial de pagos  
**Para** hacer seguimiento de la recaudación

**Criterios de aceptación:**
- Permite filtrar por membresía, socio, método de pago y rango de fechas
- Soporta paginación

---

### HU-22 — Ver detalle de un pago
**Como** usuario autenticado  
**Quiero** consultar los datos de un pago específico  
**Para** verificar su información

---

### HU-23 — Registrar un pago
**Como** admin o superadmin  
**Quiero** registrar el pago de una membresía  
**Para** saldar la deuda del socio

**Criterios de aceptación:**
- Se asocia a una membresía específica
- El monto debe ser positivo
- Se selecciona el método de pago (efectivo, transferencia, tarjeta, etc.)
- Se puede pagar parcialmente (múltiples pagos por membresía)
- Al registrar el pago se genera automáticamente un comprobante
- El saldo de la membresía se actualiza automáticamente

---

### HU-24 — Generar comprobante de pago
**Como** usuario autenticado  
**Quiero** obtener el comprobante de un pago realizado  
**Para** entregárselo al socio

**Criterios de aceptación:**
- El número de comprobante tiene el formato PAG-000123-2025
- Incluye: datos del socio, período de membresía, actividades, método de pago, monto, saldo antes y después del pago

---

### HU-25 — Anular un pago
**Como** admin o superadmin  
**Quiero** anular un pago registrado por error  
**Para** corregir el historial de cobros

**Criterios de aceptación:**
- La anulación es lógica (soft delete)
- El saldo de la membresía se recalcula automáticamente al anular

---

### HU-26 — Ver métodos de pago disponibles
**Como** usuario autenticado  
**Quiero** consultar los métodos de pago habilitados  
**Para** seleccionar el correcto al registrar un cobro

---

### HU-27 — Ver estadísticas de pagos
**Como** admin o superadmin  
**Quiero** consultar estadísticas generales de la recaudación  
**Para** analizar el estado financiero del club

**Criterios de aceptación:**
- Permite filtrar por rango de fechas
- Muestra totales recaudados, cantidad de pagos y resumen por método de pago

---

### HU-28 — Ver total recaudado
**Como** admin o superadmin  
**Quiero** ver el monto total recaudado en un período  
**Para** hacer un cierre de caja o reporte financiero

---

## Gestión de Asistencias

### HU-29 — Verificar estado de un socio por DNI
**Como** recepcionista, admin o superadmin  
**Quiero** consultar el estado de membresía de un socio ingresando su DNI  
**Para** saber si puede ingresar al club

**Criterios de aceptación:**
- Devuelve nombre del socio, estado de membresía y actividades contratadas
- Indica si la membresía está vigente o vencida

---

### HU-30 — Registrar asistencia
**Como** recepcionista, admin o superadmin  
**Quiero** registrar el ingreso de un socio ingresando su DNI  
**Para** llevar el control de asistencias

**Criterios de aceptación:**
- Se registra la fecha y hora exacta de ingreso
- El socio debe existir en el sistema

---

### HU-31 — Ver historial de asistencias
**Como** recepcionista, admin o superadmin  
**Quiero** consultar el historial de asistencias  
**Para** hacer seguimiento de la concurrencia al club

**Criterios de aceptación:**
- Permite filtrar por fecha específica y/o por socio
- Resultados ordenados por fecha descendente

---

## Gestión de Usuarios del Sistema

### HU-32 — Listar usuarios del sistema
**Como** admin o superadmin  
**Quiero** ver todos los usuarios con acceso al sistema  
**Para** controlar quién puede operar

**Criterios de aceptación:**
- Permite filtrar por rol y estado (activo/inactivo)

---

### HU-33 — Ver detalle de un usuario
**Como** admin o superadmin  
**Quiero** consultar los datos de un usuario del sistema  
**Para** verificar su configuración y permisos

---

### HU-34 — Crear un usuario del sistema
**Como** admin o superadmin  
**Quiero** crear una cuenta de acceso para otro operador  
**Para** darle acceso al sistema según su rol

**Criterios de aceptación:**
- El admin solo puede crear usuarios con rol recepcionista
- El superadmin puede crear usuarios con rol admin o recepcionista
- El nombre de usuario y email deben ser únicos

---

### HU-35 — Editar un usuario del sistema
**Como** superadmin  
**Quiero** modificar los datos de un usuario  
**Para** actualizar su información o cambiar su rol

---

### HU-36 — Desactivar un usuario del sistema
**Como** superadmin  
**Quiero** desactivar la cuenta de un usuario  
**Para** revocar su acceso sin eliminar su historial

**Criterios de aceptación:**
- La desactivación es lógica (soft delete)
- El usuario no puede iniciar sesión una vez desactivado

---

## Auditoría

### HU-37 — Registrar operaciones automáticamente
**Como** sistema  
**Quiero** registrar automáticamente cada inserción, modificación y eliminación de datos  
**Para** tener trazabilidad completa de todas las operaciones

**Criterios de aceptación:**
- Se registra: tabla afectada, tipo de operación (INSERT/UPDATE/DELETE), usuario que realizó la acción, fecha y hora, valores anteriores y nuevos

---

## Resumen de permisos por historia de usuario

| Historia | superadmin | admin | recepcionista |
|---|:---:|:---:|:---:|
| HU-01 Iniciar sesión | ✅ | ✅ | ✅ |
| HU-02 Ver mi perfil | ✅ | ✅ | ✅ |
| HU-03 Listar socios | ✅ | ✅ | ✅ |
| HU-04 Ver detalle socio | ✅ | ✅ | ✅ |
| HU-05 Dar de alta socio | ✅ | ✅ | ❌ |
| HU-06 Editar socio | ✅ | ✅ | ❌ |
| HU-07 Dar de baja socio | ✅ | ✅ | ❌ |
| HU-08 Total socios | ✅ | ✅ | ✅ |
| HU-09 Listar actividades | ✅ | ✅ | ✅ |
| HU-10 Crear actividad | ✅ | ✅ | ❌ |
| HU-11 Editar actividad | ✅ | ✅ | ❌ |
| HU-12 Eliminar actividad | ✅ | ✅ | ❌ |
| HU-13 Listar membresías | ✅ | ✅ | ✅ |
| HU-14 Ver detalle membresía | ✅ | ✅ | ✅ |
| HU-15 Crear membresía | ✅ | ✅ | ❌ |
| HU-16 Editar membresía | ✅ | ✅ | ❌ |
| HU-17 Eliminar membresía | ✅ | ✅ | ❌ |
| HU-18 Asignar actividad | ✅ | ✅ | ✅ |
| HU-19 Quitar actividad | ✅ | ✅ | ✅ |
| HU-20 Total membresías | ✅ | ✅ | ✅ |
| HU-21 Listar pagos | ✅ | ✅ | ✅ |
| HU-22 Ver detalle pago | ✅ | ✅ | ✅ |
| HU-23 Registrar pago | ✅ | ✅ | ❌ |
| HU-24 Generar comprobante | ✅ | ✅ | ✅ |
| HU-25 Anular pago | ✅ | ✅ | ❌ |
| HU-26 Ver métodos de pago | ✅ | ✅ | ✅ |
| HU-27 Estadísticas de pagos | ✅ | ✅ | ❌ |
| HU-28 Total recaudado | ✅ | ✅ | ❌ |
| HU-29 Verificar socio por DNI | ✅ | ✅ | ✅ |
| HU-30 Registrar asistencia | ✅ | ✅ | ✅ |
| HU-31 Ver historial asistencias | ✅ | ✅ | ✅ |
| HU-32 Listar usuarios sistema | ✅ | ✅ | ❌ |
| HU-33 Ver detalle usuario | ✅ | ✅ | ❌ |
| HU-34 Crear usuario sistema | ✅ | ✅ (solo recep) | ❌ |
| HU-35 Editar usuario sistema | ✅ | ❌ | ❌ |
| HU-36 Desactivar usuario | ✅ | ❌ | ❌ |
| HU-37 Auditoría automática | automático | automático | automático |
