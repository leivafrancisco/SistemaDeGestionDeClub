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

**Dado** que soy admin o superadmin y tengo un socio activo  
**Cuando** ingreso socio, fecha de inicio, fecha de fin y al menos una actividad  
**Entonces** el sistema crea la membresía y congela el precio de cada actividad al momento de la creación

**Dado** que intento crear una membresía  
**Cuando** la fecha de fin es igual o anterior a la fecha de inicio  
**Entonces** el sistema rechaza la operación con un mensaje de error

**Dado** que el socio ya tiene una membresía en ese rango de fechas  
**Cuando** intento crear una nueva membresía que se superpone  
**Entonces** el sistema rechaza la operación indicando el solapamiento

**Dado** que selecciono las actividades para la membresía  
**Cuando** el sistema crea la membresía  
**Entonces** el costo total se calcula automáticamente como la suma de los precios de las actividades seleccionadas

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

**Dado** que la membresía no tiene pagos registrados  
**Cuando** solicito su eliminación  
**Entonces** el sistema realiza un soft delete (marca fecha_eliminacion) y la membresía deja de aparecer en los listados

**Dado** que la membresía tiene al menos un pago registrado  
**Cuando** intento eliminarla  
**Entonces** el sistema rechaza la operación con un mensaje de error indicando que no se puede eliminar una membresía con pagos

**Dado** que la membresía tiene cuotas pero ningún pago  
**Cuando** solicito su eliminación  
**Entonces** el sistema la elimina junto con sus cuotas asociadas (cascade delete)

**Dado** que la membresía no existe o ya fue eliminada  
**Cuando** intento eliminarla  
**Entonces** el sistema responde con un error de recurso no encontrado

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
**Quiero** registrar el pago de una cuota de membresía  
**Para** saldar la deuda mensual del socio

**Criterios de aceptación:**

**Dado** que soy admin o superadmin y existe una cuota en estado pendiente o vencida  
**Cuando** registro un pago indicando la cuota, el monto y el método de pago  
**Entonces** el sistema registra el pago, marca la cuota como pagada, actualiza el saldo de la membresía y genera automáticamente un comprobante

**Dado** que ingreso un monto igual o menor a cero  
**Cuando** intento registrar el pago  
**Entonces** el sistema rechaza la operación con un mensaje de error indicando que el monto debe ser mayor a cero

**Dado** que la cuota indicada ya tiene estado pagada  
**Cuando** intento registrar un nuevo pago sobre esa cuota  
**Entonces** el sistema rechaza la operación indicando que la cuota ya fue pagada

**Dado** que registro un pago exitoso  
**Cuando** el sistema procesa la operación  
**Entonces** se genera un comprobante con número único en formato PAG-000001-2026 que incluye datos del socio, período de membresía, actividades, método de pago y saldo resultante

**Dado** que anulo un pago que estaba asociado a una cuota  
**Cuando** el sistema procesa la anulación  
**Entonces** la cuota vuelve al estado pendiente si su fecha de vencimiento no pasó, o vencida si ya venció

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

## Gestión de Cuotas

### HU-38 — Listar cuotas con filtros
**Como** usuario autenticado  
**Quiero** consultar el listado de cuotas mensuales  
**Para** hacer seguimiento del estado de cobro de cada período

**Criterios de aceptación:**
- Permite filtrar por membresía, socio, estado (`pendiente`, `pagada`, `vencida`) y rango de fecha de vencimiento
- Permite filtrar solo cuotas vencidas
- Muestra: número de cuota, monto, fecha de vencimiento, estado y nombre del socio
- Soporta paginación

---

### HU-39 — Ver cuotas de una membresía
**Como** usuario autenticado  
**Quiero** ver todas las cuotas generadas para una membresía específica  
**Para** conocer el detalle de los períodos de cobro y su estado

**Criterios de aceptación:**
- Se listan ordenadas por número de cuota
- Muestra monto, fecha de vencimiento y estado de cada cuota
- Indica claramente cuáles están vencidas (morosas)

---

### HU-40 — Ver cuotas de un socio
**Como** usuario autenticado  
**Quiero** ver todas las cuotas de un socio a través de todas sus membresías  
**Para** tener una vista consolidada de su situación de pago

**Criterios de aceptación:**
- Muestra cuotas de todas las membresías del socio
- Ordenadas por fecha de vencimiento descendente
- Indica el período de membresía al que pertenece cada cuota

---

### HU-41 — Ver listado de socios morosos
**Como** admin o superadmin  
**Quiero** obtener el listado de socios con cuotas vencidas  
**Para** tomar acciones de cobranza y notificación

**Criterios de aceptación:**
- Solo muestra socios con al menos una cuota en estado `vencida`
- Por cada socio moroso muestra: nombre, número de socio, email, cantidad de cuotas vencidas, deuda total y fecha del vencimiento más antiguo
- Ordenado por deuda total de mayor a menor
- Incluye el detalle de cada cuota vencida por socio

---

### HU-42 — Ver resumen general de cuotas
**Como** admin o superadmin  
**Quiero** ver un resumen del estado de todas las cuotas del sistema  
**Para** tener una visión global de la situación de cobro del club

**Criterios de aceptación:**
- Muestra cantidad y monto total de cuotas: pendientes, pagadas y vencidas
- Muestra el monto total vencido (deuda morosa acumulada)
- Muestra el total de socios morosos

---

### HU-43 — Generar cuotas para una membresía existente
**Como** admin o superadmin  
**Quiero** generar manualmente las cuotas mensuales de una membresía  
**Para** cuando la membresía fue creada antes de implementar el sistema de cuotas

**Criterios de aceptación:**

**Dado** que la membresía no tiene cuotas generadas  
**Cuando** solicito la generación de cuotas  
**Entonces** el sistema crea una cuota por cada mes comprendido entre la fecha de inicio y la fecha de fin de la membresía, con monto proporcional al costo total y fecha de vencimiento al último día de cada mes

**Dado** que la membresía ya tiene cuotas generadas  
**Cuando** intento regenerar las cuotas  
**Entonces** el sistema rechaza la operación con un mensaje de error

**Nota:** Al crear una membresía nueva, las cuotas se generan automáticamente sin necesidad de invocar este endpoint.

---

### HU-44 — Actualizar cuotas vencidas
**Como** admin o superadmin  
**Quiero** ejecutar la actualización de estados de cuotas  
**Para** que las cuotas con fecha de vencimiento pasada queden marcadas como vencidas

**Criterios de aceptación:**
- Recorre todas las cuotas en estado `pendiente`
- Marca como `vencida` aquellas cuya fecha de vencimiento sea anterior a la fecha actual
- Devuelve la cantidad de cuotas actualizadas en la respuesta
- Esta operación puede ejecutarse manualmente o programarse periódicamente

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
**Como** superadmin  
**Quiero** crear una cuenta de acceso para otro operador  
**Para** darle acceso al sistema según su rol

**Criterios de aceptación:**

**Dado** que soy superadmin  
**Cuando** creo un nuevo usuario con rol admin o recepcionista  
**Entonces** el sistema crea la cuenta y el usuario puede iniciar sesión con sus credenciales

**Dado** que ingreso un nombre de usuario o email que ya existe  
**Cuando** intento crear el nuevo usuario  
**Entonces** el sistema rechaza la operación indicando que el nombre de usuario o email ya está en uso

**Dado** que soy admin o recepcionista  
**Cuando** intento crear un nuevo usuario  
**Entonces** el sistema rechaza la operación por falta de permisos

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
| HU-38 Listar cuotas | ✅ | ✅ | ✅ |
| HU-39 Ver cuotas de membresía | ✅ | ✅ | ✅ |
| HU-40 Ver cuotas de socio | ✅ | ✅ | ✅ |
| HU-41 Listado de morosos | ✅ | ✅ | ❌ |
| HU-42 Resumen general cuotas | ✅ | ✅ | ❌ |
| HU-43 Generar cuotas manualmente | ✅ | ✅ | ❌ |
| HU-44 Actualizar cuotas vencidas | ✅ | ✅ | ❌ |
| HU-29 Verificar socio por DNI | ✅ | ✅ | ✅ |
| HU-30 Registrar asistencia | ✅ | ✅ | ✅ |
| HU-31 Ver historial asistencias | ✅ | ✅ | ✅ |
| HU-32 Listar usuarios sistema | ✅ | ✅ | ❌ |
| HU-33 Ver detalle usuario | ✅ | ✅ | ❌ |
| HU-34 Crear usuario sistema | ✅ | ✅ (solo recep) | ❌ |
| HU-35 Editar usuario sistema | ✅ | ❌ | ❌ |
| HU-36 Desactivar usuario | ✅ | ❌ | ❌ |
| HU-37 Auditoría automática | automático | automático | automático |
