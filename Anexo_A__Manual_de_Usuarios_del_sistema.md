# Anexo A - Manual de Usuarios del Sistema de Gestión de Socios del Club

---

## Introducción

El propósito de este documento es describir la utilización por parte de los usuarios del Sistema de Gestión de Socios del Club. El sistema permite administrar el ciclo de vida completo de los socios: desde su alta, la gestión de membresías y actividades, el registro de pagos y cuotas, hasta el control de asistencia al club.

---

## Objetivo de este manual

El objetivo es indicar los pasos y procedimientos a realizar para llevar a cabo las distintas tareas y funcionalidades que provee el sistema, de modo que los usuarios puedan operar el mismo de manera eficiente y correcta.

---

## Dirigido a

Para la utilización del presente sistema se pueden reconocer tres distintos tipos de perfiles:

**Recepcionista**: es el usuario encargado del control de acceso al club. Puede verificar el estado de membresía de un socio mediante su DNI, registrar asistencias y asignar o remover actividades en membresías existentes.

**Administrador (Admin)**: es el usuario interno de la organización que, además de las funciones de recepcionista, puede gestionar socios (alta, modificación, baja), crear y administrar membresías, registrar pagos, consultar cuotas, generar reportes de morosos y obtener estadísticas de recaudación.

**Superadministrador (Superadmin)**: es el usuario con el mayor nivel de acceso. Además de todas las funciones del administrador, puede gestionar el catálogo de actividades, crear y administrar usuarios del sistema, acceder al registro de auditoría completo y realizar copias de seguridad (backup) y restauraciones de la base de datos.

---

## Lo que deben conocer

Los conocimientos mínimos que deben tener las personas que operarán el sistema son:

**Recepcionistas**: deben conocer el procedimiento de control de acceso al club y contar con conocimientos básicos en el manejo de interfaces web con formularios y menús.

**Administradores**: deben conocer el manual de procedimientos del club en cuanto a la gestión de socios, cobro de cuotas y altas/bajas de membresías. Deben manejar con fluidez navegadores web y herramientas similares.

**Superadministrador**: debe tener conocimientos profundos de la estructura y funcionamiento del club. Además es deseable que conozca aspectos fundamentales del sistema operativo del servidor, configuración de redes y administración de bases de datos.

---

## Especificaciones técnicas

Para la implementación y uso del sistema se deberá contar con los siguientes requerimientos:

### Hardware (servidor)

- CPU de al menos 2 GHz (se recomiendan 4 núcleos).
- 8 GB de memoria RAM como mínimo (se recomiendan 16 GB).
- 100 GB de disco duro disponible como mínimo.
- Conectividad de red (Ethernet o Wi-Fi) hacia los clientes.
- Se recomienda contar con una UPS para evitar posibles problemas energéticos.

### Hardware (clientes)

- Computadora, notebook o dispositivo con acceso a un navegador web moderno.
- Conexión de red hacia el servidor donde está alojado el sistema.

### Software (servidor)

- Sistema operativo: Linux o Windows Server (ambos compatibles con Docker).
- Docker y Docker Compose (para el despliegue en contenedores).
- SQL Server 2019 o superior.
- .NET 8.0 Runtime.

### Software (clientes)

- Navegador web moderno: Google Chrome, Mozilla Firefox, Microsoft Edge o Safari (versiones actuales).
- No se requiere instalación adicional en los equipos cliente.

---

## Características del producto

El sistema fue desarrollado con tecnología **ASP.NET Core 8.0** y una base de datos **SQL Server**, siguiendo una arquitectura en capas (API, Aplicación, Dominio e Infraestructura) que facilita su mantenimiento y escalabilidad.

El sistema ofrece las siguientes características:

- **Interfaz de tipo API REST**: accesible desde cualquier cliente con navegador web o herramienta compatible con HTTP.
- **Autenticación segura**: mediante tokens JWT (JSON Web Tokens) con expiración configurable.
- **Control de acceso por roles**: cada tipo de usuario (Recepcionista, Admin, Superadmin) tiene habilitadas únicamente las funciones correspondientes a su perfil.
- **Registro de auditoría**: todas las operaciones de creación, modificación y eliminación quedan registradas con el usuario que las realizó y los valores anteriores y nuevos.
- **Eliminación lógica (soft delete)**: los socios, membresías y usuarios nunca se borran físicamente de la base de datos; se marcan como inactivos para preservar el historial.
- **Despliegue con Docker**: el sistema puede levantarse fácilmente en cualquier servidor compatible con Docker mediante `docker-compose up`.

---

## Uso del sistema

### Ingreso al sistema (Login)

Para acceder al sistema, el usuario debe realizar una solicitud de autenticación al endpoint correspondiente, proporcionando su **nombre de usuario** y **contraseña**.

**Endpoint:** `POST /api/auth/login`

**Datos requeridos:**

| Campo            | Descripción                              |
|-----------------|------------------------------------------|
| `nombreUsuario` | Nombre de usuario asignado por el admin  |
| `contrasena`    | Contraseña del usuario                   |

**Respuesta exitosa:** el sistema devuelve un **token JWT** que deberá incluirse en todas las solicitudes posteriores en el encabezado `Authorization: Bearer <token>`.

**Errores posibles:**

- `401 Unauthorized`: credenciales inválidas. Verificar nombre de usuario y contraseña.

> **Nota:** Si el usuario ingresa credenciales incorrectas o su cuenta fue desactivada, el sistema rechazará el acceso. En ese caso debe comunicarse con el administrador del sistema.

---

### Consultar perfil del usuario actual

Una vez autenticado, cualquier usuario puede consultar su propia información.

**Endpoint:** `GET /api/auth/me`

**Respuesta:** nombre completo, nombre de usuario, rol asignado y estado de la cuenta.

---

### Actualizar perfil propio

Cualquier usuario autenticado puede actualizar sus propios datos de perfil (incluyendo cambio de contraseña).

**Endpoint:** `PUT /api/auth/perfil`

**Datos opcionales:**

| Campo                | Descripción                                        |
|---------------------|----------------------------------------------------|
| `nombre`            | Nuevo nombre                                       |
| `apellido`          | Nuevo apellido                                     |
| `email`             | Nuevo correo electrónico                           |
| `contrasenaActual`  | Contraseña actual (requerida para cambiar contraseña) |
| `nuevaContrasena`   | Nueva contraseña deseada                           |

> **Importante:** para cambiar la contraseña es obligatorio proporcionar la contraseña actual. Si se ingresa una contraseña actual incorrecta, el sistema devolverá un error `400 Bad Request`.

---

## Gestión de Socios

Los socios son los miembros del club. La gestión de socios está disponible para los roles **Admin** y **Superadmin**.

### Listar socios

Permite obtener el listado de socios con filtros opcionales.

**Endpoint:** `GET /api/socios`

**Parámetros opcionales:**

| Parámetro    | Descripción                                              |
|-------------|----------------------------------------------------------|
| `search`    | Búsqueda por nombre, apellido, DNI o número de socio     |
| `estaActivo`| Filtrar por estado: `true` (activos) o `false` (inactivos) |
| `page`      | Número de página (por defecto: 1)                        |
| `pageSize`  | Cantidad de resultados por página (por defecto: 20)      |

**Respuesta:** lista de socios con los campos: ID, número de socio, nombre, apellido, email, DNI, fecha de nacimiento, estado activo, fecha de alta y fecha de baja.

---

### Buscar socio por número de socio

**Endpoint:** `GET /api/socios/numero/{numeroSocio}`

**Parámetro de ruta:** `numeroSocio` — número de socio asignado por el sistema.

**Respuesta:** datos completos del socio o error `404` si no se encuentra.

---

### Consultar socio por ID

**Endpoint:** `GET /api/socios/{id}`

**Parámetro de ruta:** `id` — identificador interno del socio.

---

### Crear un nuevo socio

Registra un nuevo socio en el sistema. El número de socio es generado automáticamente.

**Endpoint:** `POST /api/socios`

**Roles:** Admin, Superadmin.

**Datos requeridos:**

| Campo            | Descripción                        | Obligatorio |
|-----------------|------------------------------------|-------------|
| `nombre`        | Nombre del socio                   | Sí          |
| `apellido`      | Apellido del socio                 | Sí          |
| `email`         | Correo electrónico                 | Sí          |
| `dni`           | Documento Nacional de Identidad    | No          |
| `fechaNacimiento`| Fecha de nacimiento (YYYY-MM-DD)  | No          |

**Respuesta exitosa:** datos del socio creado, incluyendo el número de socio asignado automáticamente.

**Errores posibles:**

- `400 Bad Request`: ya existe un socio con el mismo DNI o email.

---

### Modificar datos de un socio

Actualiza la información personal de un socio existente.

**Endpoint:** `PUT /api/socios/{id}`

**Roles:** Admin, Superadmin.

**Datos:** mismos campos que en la creación (todos opcionales, se actualizan solo los enviados).

---

### Dar de baja a un socio

Desactiva un socio (baja lógica). El socio no se elimina de la base de datos sino que se marca como inactivo.

**Endpoint:** `PUT /api/socios/{id}/desactivar`

**Roles:** Admin, Superadmin.

**Respuesta exitosa:** `{ "message": "Socio desactivado exitosamente" }`.

---

### Estadísticas de socios

Obtiene la cantidad total de socios activos.

**Endpoint:** `GET /api/socios/estadisticas/total`

**Respuesta:** `{ "total": <número> }`.

---

## Gestión de Membresías

Las membresías representan el contrato activo de un socio con el club, incluyendo las actividades a las que está inscripto y el período de vigencia.

### Listar membresías

**Endpoint:** `GET /api/membresias`

**Parámetros opcionales:**

| Parámetro       | Descripción                                                        |
|----------------|--------------------------------------------------------------------|
| `idSocio`      | Filtrar por ID de socio                                            |
| `fechaDesde`   | Fecha de inicio del rango de búsqueda                              |
| `fechaHasta`   | Fecha de fin del rango de búsqueda                                 |
| `soloImpagas`  | `true` para mostrar solo membresías con cuotas pendientes          |
| `search`       | Búsqueda por nombre de socio o número de socio                     |
| `estadoVigencia`| `vigentes`, `vencidas`, `proximas_vencer` o `todas`              |
| `page`         | Número de página                                                   |
| `pageSize`     | Resultados por página                                              |

**Respuesta:** lista de membresías con datos del socio, fechas de inicio y fin, costo total, total pagado, saldo pendiente, estado y actividades asociadas.

---

### Consultar membresía por ID

**Endpoint:** `GET /api/membresias/{id}`

**Respuesta:** datos completos de la membresía incluyendo actividades, montos y estado de pago.

---

### Crear una nueva membresía

Registra una nueva membresía para un socio. Al crear la membresía se registra también un pago inicial.

**Endpoint:** `POST /api/membresias`

**Roles:** Admin, Superadmin.

**Datos requeridos:**

| Campo             | Descripción                                                  | Obligatorio |
|------------------|--------------------------------------------------------------|-------------|
| `idSocio`        | ID del socio al que pertenece la membresía                   | Sí          |
| `fechaInicio`    | Fecha de inicio de la membresía (YYYY-MM-DD)                 | Sí          |
| `fechaFin`       | Fecha de fin de la membresía (YYYY-MM-DD)                    | Sí          |
| `costoTotal`     | Costo total de la membresía                                  | Sí          |
| `idsActividades` | Lista de IDs de actividades incluidas                        | Sí          |
| `monto`          | Monto del pago inicial                                       | Sí          |
| `idMetodoPago`   | ID del método de pago utilizado                              | Sí          |
| `idUsuarioProcesa`| ID del usuario que procesa el pago                          | Sí          |

**Errores posibles:**

- `400 Bad Request`: el socio no existe, está inactivo, o ya tiene una membresía vigente en ese período.

---

### Actualizar una membresía

Permite modificar las fechas de inicio/fin o las actividades asociadas. Los pagos existentes se mantienen intactos y el saldo se recalcula automáticamente.

**Endpoint:** `PUT /api/membresias/{id}`

**Roles:** Admin, Superadmin.

**Datos opcionales:**

| Campo            | Descripción                          |
|-----------------|--------------------------------------|
| `fechaInicio`   | Nueva fecha de inicio                |
| `fechaFin`      | Nueva fecha de fin                   |
| `idsActividades`| Nuevo listado de IDs de actividades  |

> **Nota:** si el saldo resultante tras la actualización es negativo, significa que el socio tiene un saldo a favor que deberá ser considerado en el próximo cobro.

---

### Eliminar una membresía

Realiza una eliminación lógica (soft delete) de la membresía. Solo puede ejecutarse si no hay pagos registrados contra ella.

**Endpoint:** `DELETE /api/membresias/{id}`

**Roles:** Admin, Superadmin.

---

### Asignar actividad a una membresía

Agrega una actividad a una membresía existente.

**Endpoint:** `POST /api/membresias/asignar-actividad`

**Roles:** Recepcionista, Admin, Superadmin.

**Datos requeridos:**

| Campo         | Descripción                   |
|--------------|-------------------------------|
| `idMembresia`| ID de la membresía            |
| `idActividad`| ID de la actividad a asignar  |

---

### Remover actividad de una membresía

Quita una actividad de una membresía. Solo es posible si no hay pagos registrados relacionados con esa actividad.

**Endpoint:** `POST /api/membresias/remover-actividad`

**Roles:** Recepcionista, Admin, Superadmin.

**Datos requeridos:**

| Campo         | Descripción                   |
|--------------|-------------------------------|
| `idMembresia`| ID de la membresía            |
| `idActividad`| ID de la actividad a remover  |

---

## Gestión de Actividades

Las actividades son los servicios o disciplinas que ofrece el club (por ejemplo: natación, tenis, gym). Solo el **Superadmin** puede crear, modificar o eliminar actividades. Todos los roles autenticados pueden consultarlas.

### Listar actividades

**Endpoint:** `GET /api/actividades`

**Respuesta:** lista de actividades con ID, nombre, descripción, precio actual e indicador de si es cuota base.

---

### Consultar actividad por ID

**Endpoint:** `GET /api/actividades/{id}`

---

### Crear actividad

**Endpoint:** `POST /api/actividades`

**Roles:** Superadmin.

**Datos requeridos:**

| Campo          | Descripción                                              | Obligatorio |
|---------------|----------------------------------------------------------|-------------|
| `nombre`      | Nombre de la actividad                                   | Sí          |
| `descripcion` | Descripción de la actividad                              | No          |
| `precioActual`| Precio vigente de la actividad                           | Sí          |
| `esCuotaBase` | Indica si es parte de la cuota base del club             | Sí          |

**Errores posibles:**

- `400 Bad Request`: ya existe una actividad con el mismo nombre.

---

### Modificar actividad

**Endpoint:** `PUT /api/actividades/{id}`

**Roles:** Superadmin.

**Datos:** mismos campos que en la creación.

---

### Eliminar actividad

Realiza una eliminación lógica de la actividad.

**Endpoint:** `DELETE /api/actividades/{id}`

**Roles:** Superadmin.

---

## Gestión de Cuotas

Las cuotas son las obligaciones de pago mensuales generadas a partir de una membresía. Cada cuota corresponde a un mes dentro del período de la membresía.

### Listar cuotas

**Endpoint:** `GET /api/cuotas`

**Parámetros opcionales:**

| Parámetro              | Descripción                                         |
|-----------------------|-----------------------------------------------------|
| `idMembresia`         | Filtrar por membresía                               |
| `idSocio`             | Filtrar por socio                                   |
| `estado`              | Estado de la cuota: `pendiente`, `pagada`, `vencida`|
| `soloVencidas`        | `true` para mostrar solo cuotas vencidas            |
| `fechaVencimientoDesde`| Rango de fecha de vencimiento (inicio)             |
| `fechaVencimientoHasta`| Rango de fecha de vencimiento (fin)               |
| `page`                | Número de página                                    |
| `pageSize`            | Resultados por página                               |

---

### Consultar cuota por ID

**Endpoint:** `GET /api/cuotas/{id}`

---

### Cuotas de una membresía

Obtiene todas las cuotas asociadas a una membresía específica.

**Endpoint:** `GET /api/cuotas/membresia/{idMembresia}`

---

### Cuotas de un socio

Obtiene todas las cuotas asociadas a un socio específico (a través de todas sus membresías).

**Endpoint:** `GET /api/cuotas/socio/{idSocio}`

---

### Generar cuotas para una membresía

Genera automáticamente las cuotas mensuales para una membresía existente, una por cada mes comprendido entre la fecha de inicio y la fecha de fin.

**Endpoint:** `POST /api/cuotas/generar/{idMembresia}`

**Roles:** Admin, Superadmin.

**Errores posibles:**

- `400 Bad Request`: la membresía no existe o ya tiene cuotas generadas.

---

### Actualizar cuotas vencidas

Marca automáticamente como **vencidas** todas las cuotas pendientes cuya fecha de vencimiento ya ha pasado.

**Endpoint:** `POST /api/cuotas/actualizar-vencidas`

**Roles:** Admin, Superadmin.

**Respuesta:** `{ "message": "Se marcaron N cuota(s) como vencidas", "cantidad": N }`.

---

### Resumen de cuotas

Obtiene un resumen general del estado de cuotas del club (total pendientes, total vencidas, monto total adeudado, etc.).

**Endpoint:** `GET /api/cuotas/resumen`

**Roles:** Admin, Superadmin.

---

### Listado de socios morosos

Obtiene el listado de socios que tienen cuotas vencidas sin pagar.

**Endpoint:** `GET /api/cuotas/morosos`

**Roles:** Admin, Superadmin.

**Respuesta:** lista con nombre del socio, número de socio, cantidad de cuotas vencidas y monto total adeudado.

---

## Gestión de Pagos

El módulo de pagos permite registrar los cobros realizados por cuotas de membresía y consultar el historial de pagos.

### Listar pagos

**Endpoint:** `GET /api/pagos`

**Parámetros opcionales:**

| Parámetro      | Descripción                             |
|---------------|-----------------------------------------|
| `idMembresia` | Filtrar por membresía                   |
| `idSocio`     | Filtrar por socio                       |
| `idMetodoPago`| Filtrar por método de pago              |
| `fechaDesde`  | Rango de fecha de pago (inicio)         |
| `fechaHasta`  | Rango de fecha de pago (fin)            |
| `page`        | Número de página                        |
| `pageSize`    | Resultados por página                   |

---

### Consultar pago por ID

**Endpoint:** `GET /api/pagos/{id}`

---

### Registrar un pago

Registra un nuevo pago para una cuota. Al registrar el pago, el sistema actualiza automáticamente el estado de la cuota y genera un comprobante.

**Endpoint:** `POST /api/pagos`

**Roles:** Admin, Superadmin.

**Datos requeridos:**

| Campo         | Descripción                             | Obligatorio |
|--------------|-----------------------------------------|-------------|
| `idCuota`    | ID de la cuota que se está pagando      | Sí          |
| `idMetodoPago`| ID del método de pago utilizado        | Sí          |
| `monto`      | Monto abonado                           | Sí          |

**Respuesta exitosa:** comprobante de pago con número de comprobante, datos del socio, cuota, monto y fecha.

**Errores posibles:**

- `400 Bad Request`: la cuota ya fue pagada o el monto es inválido.

---

### Obtener comprobante de un pago

Genera o recupera el comprobante de un pago ya registrado.

**Endpoint:** `GET /api/pagos/{id}/comprobante`

---

### Anular un pago

Realiza una anulación lógica del pago. La cuota vuelve a quedar en estado pendiente.

**Endpoint:** `DELETE /api/pagos/{id}`

**Roles:** Admin, Superadmin.

---

### Métodos de pago disponibles

Obtiene la lista de métodos de pago configurados en el sistema (efectivo, transferencia, tarjeta, etc.).

**Endpoint:** `GET /api/pagos/metodos`

---

### Estadísticas de pagos

Obtiene estadísticas completas de pagos (total recaudado, cantidad de pagos, distribución por método de pago, etc.) en un rango de fechas opcional.

**Endpoint:** `GET /api/pagos/estadisticas`

**Roles:** Admin, Superadmin.

**Parámetros opcionales:** `fechaDesde`, `fechaHasta`.

---

### Recaudación en un período

Obtiene el total recaudado en un rango de fechas.

**Endpoint:** `GET /api/pagos/estadisticas/recaudacion`

**Roles:** Admin, Superadmin.

**Parámetros opcionales:** `fechaDesde`, `fechaHasta`.

---

## Control de Asistencia

El módulo de asistencia permite verificar el estado de membresía de un socio por su DNI y registrar su ingreso al club. Este módulo es utilizado principalmente por **Recepcionistas**.

### Verificar estado de membresía por DNI

Consulta si un socio tiene membresía vigente al presentar su DNI, sin registrar el ingreso todavía.

**Endpoint:** `GET /api/asistencias/verificar/{dni}`

**Parámetro de ruta:** `dni` — número de documento del socio.

**Respuesta exitosa:** nombre del socio, número de socio, estado de la membresía (vigente/vencida) y actividades habilitadas.

**Errores posibles:**

- `404 Not Found`: no existe ningún socio con ese DNI.

---

### Registrar asistencia (check-in)

Registra el ingreso del socio al club. El sistema valida que la membresía esté vigente antes de permitir el ingreso.

**Endpoint:** `POST /api/asistencias/registrar/{dni}`

**Parámetro de ruta:** `dni` — número de documento del socio.

**Respuesta exitosa:** registro de asistencia con ID del socio, fecha y hora de ingreso.

**Errores posibles:**

- `400 Bad Request`: el socio no tiene membresía vigente o su estado no le permite el ingreso.

---

### Historial de asistencias

Consulta el historial de ingresos al club con filtros opcionales.

**Endpoint:** `GET /api/asistencias`

**Roles:** Recepcionista, Admin, Superadmin.

**Parámetros opcionales:**

| Parámetro | Descripción                                  |
|----------|----------------------------------------------|
| `fecha`  | Filtrar por fecha específica (formato YYYY-MM-DD) |
| `idSocio`| Filtrar por socio específico                 |

**Respuesta:** lista de asistencias ordenadas por fecha descendente.

---

## Gestión de Usuarios del Sistema

La gestión de usuarios del sistema permite crear y administrar las cuentas de acceso para el personal del club. Solo **Admin** y **Superadmin** tienen acceso a este módulo.

### Listar usuarios

**Endpoint:** `GET /api/usuarios`

**Roles:** Admin, Superadmin.

**Parámetros opcionales:**

| Parámetro   | Descripción                                              |
|------------|----------------------------------------------------------|
| `rol`      | Filtrar por rol: `admin`, `recepcionista`, `superadmin`  |
| `estaActivo`| Filtrar por estado: `true` o `false`                   |

---

### Consultar usuario por ID

**Endpoint:** `GET /api/usuarios/{id}`

**Roles:** Admin, Superadmin.

---

### Crear usuario

Registra un nuevo usuario en el sistema. Un **Admin** puede crear usuarios con rol **Recepcionista**. Un **Superadmin** puede crear usuarios con rol **Admin** o **Recepcionista**.

**Endpoint:** `POST /api/usuarios`

**Roles:** Admin, Superadmin.

**Datos requeridos:**

| Campo           | Descripción                                            | Obligatorio |
|----------------|--------------------------------------------------------|-------------|
| `nombre`       | Nombre de la persona                                   | Sí          |
| `apellido`     | Apellido de la persona                                 | Sí          |
| `email`        | Correo electrónico                                     | Sí          |
| `nombreUsuario`| Nombre de usuario para el login                        | Sí          |
| `contrasena`   | Contraseña inicial                                     | Sí          |
| `idRol`        | ID del rol a asignar                                   | Sí          |

**Errores posibles:**

- `400 Bad Request`: datos inválidos o faltantes.
- `409 Conflict`: ya existe un usuario con ese nombre de usuario o email.
- `403 Forbidden`: el usuario actual no tiene permisos para asignar ese rol.

---

### Actualizar usuario

Modifica los datos de un usuario existente.

**Endpoint:** `PUT /api/usuarios/{id}`

**Roles:** Superadmin.

---

### Desactivar usuario

Desactiva un usuario (baja lógica). El usuario no podrá iniciar sesión hasta que sea reactivado.

**Endpoint:** `PUT /api/usuarios/{id}/desactivar`

**Roles:** Superadmin.

---

## Registro de Auditoría

El módulo de auditoría almacena un historial completo de todas las operaciones realizadas sobre los datos del sistema (inserciones, modificaciones y eliminaciones). Solo el **Superadmin** tiene acceso.

**Endpoint:** `GET /api/auditoria`

**Respuesta:** lista de registros de auditoría con los siguientes campos:

| Campo              | Descripción                                      |
|-------------------|--------------------------------------------------|
| `tabla`           | Nombre de la tabla afectada                      |
| `operacion`       | Tipo de operación: `INSERT`, `UPDATE`, `DELETE`  |
| `idUsuario`       | ID del usuario que realizó la operación          |
| `fechaHora`       | Fecha y hora de la operación                     |
| `valoresAnteriores`| Valores previos a la modificación (JSON)        |
| `valoresNuevos`   | Valores tras la modificación (JSON)              |

---

## Backup y Restauración

El módulo de backup permite crear copias de seguridad de la base de datos y restaurarlas en caso de necesidad. Exclusivo para el rol **Superadmin**.

> **Advertencia:** la restauración de un backup sobreescribe los datos actuales de la base de datos. Esta operación es irreversible. Debe realizarse con extrema precaución y solo en situaciones justificadas.

### Listar bases de datos disponibles

Obtiene la lista de bases de datos disponibles en el servidor SQL.

**Endpoint:** `GET /api/backup/bases-datos`

---

### Listar archivos de backup disponibles

Obtiene la lista de archivos de backup almacenados en el servidor.

**Endpoint:** `GET /api/backup/archivos`

---

### Crear backup

Genera una copia de seguridad de la base de datos especificada.

**Endpoint:** `POST /api/backup`

**Datos requeridos:**

| Campo            | Descripción                                |
|-----------------|--------------------------------------------|
| `nombreBaseDatos`| Nombre de la base de datos a respaldar    |
| `rutaDestino`   | Ruta en el servidor donde se guardará el backup |
| `nombreArchivo` | Nombre del archivo de backup a generar    |

**Respuesta exitosa:** ruta completa del archivo generado, tamaño y resultado de la operación.

---

### Descargar archivo de backup

Descarga un archivo de backup existente desde el servidor.

**Endpoint:** `POST /api/backup/descargar`

**Datos requeridos:**

| Campo         | Descripción                          |
|--------------|--------------------------------------|
| `rutaCompleta`| Ruta completa del archivo de backup |

---

### Restaurar backup

Restaura la base de datos a partir de un archivo de backup.

**Endpoint:** `POST /api/backup/restaurar`

**Datos requeridos:**

| Campo            | Descripción                                           |
|-----------------|-------------------------------------------------------|
| `nombreBaseDatos`| Nombre de la base de datos destino de la restauración |
| `rutaArchivo`   | Ruta del archivo de backup a restaurar                |

---

## Tabla resumen de permisos por rol

| Funcionalidad                        | Recepcionista | Admin | Superadmin |
|-------------------------------------|:-------------:|:-----:|:----------:|
| Login / Ver perfil propio           | ✓             | ✓     | ✓          |
| Actualizar perfil propio            | ✓             | ✓     | ✓          |
| Listar socios                       | ✓             | ✓     | ✓          |
| Crear / modificar / desactivar socio|               | ✓     | ✓          |
| Listar / consultar membresías       | ✓             | ✓     | ✓          |
| Crear / modificar / eliminar membresía|             | ✓     | ✓          |
| Asignar / remover actividad en membresía| ✓         | ✓     | ✓          |
| Listar / consultar actividades      | ✓             | ✓     | ✓          |
| Crear / modificar / eliminar actividad|            |       | ✓          |
| Listar / consultar cuotas           | ✓             | ✓     | ✓          |
| Generar cuotas / actualizar vencidas|              | ✓     | ✓          |
| Resumen de cuotas / morosos         |               | ✓     | ✓          |
| Listar / consultar pagos            | ✓             | ✓     | ✓          |
| Registrar pago / anular pago        |               | ✓     | ✓          |
| Estadísticas de pagos / recaudación |               | ✓     | ✓          |
| Verificar asistencia por DNI        | ✓             | ✓     | ✓          |
| Registrar asistencia (check-in)     | ✓             | ✓     | ✓          |
| Historial de asistencias            | ✓             | ✓     | ✓          |
| Gestionar usuarios del sistema      |               | ✓*    | ✓          |
| Registro de auditoría               |               |       | ✓          |
| Backup y restauración               |               |       | ✓          |

> *El Admin puede crear usuarios con rol Recepcionista únicamente. El Superadmin puede crear usuarios Admin o Recepcionista.

---

## Glosario

| Término        | Definición                                                                                  |
|---------------|---------------------------------------------------------------------------------------------|
| **Socio**     | Persona registrada como miembro del club.                                                   |
| **Membresía** | Contrato de vigencia entre un socio y el club, con actividades y período definidos.         |
| **Actividad** | Servicio o disciplina ofrecida por el club (natación, tenis, gym, etc.).                    |
| **Cuota**     | Obligación de pago mensual generada a partir de una membresía.                              |
| **Pago**      | Registro del cobro efectivo de una cuota.                                                   |
| **Asistencia**| Registro del ingreso de un socio al club.                                                   |
| **JWT**       | JSON Web Token — mecanismo de autenticación utilizado para verificar la identidad del usuario.|
| **Soft delete**| Eliminación lógica: el registro se marca como inactivo pero no se borra físicamente.       |
| **Moroso**    | Socio con una o más cuotas vencidas sin pagar.                                              |
| **Backup**    | Copia de seguridad de la base de datos.                                                     |
| **API REST**  | Interfaz de programación que permite la comunicación entre el cliente y el servidor mediante HTTP. |
