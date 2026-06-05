# Sistema de Gestión de Club — Funciones del Sistema

## Descripción General

Sistema web integral para la administración de clubes de socios, diseñado para digitalizar y centralizar la gestión de membresías, pagos, actividades y asistencias. Accesible desde cualquier dispositivo con conexión a internet.

---

## Funciones Implementadas

### Gestión de Socios
- Alta, baja y modificación de socios
- Asignación automática de número de socio
- Historial completo por socio
- Búsqueda por nombre, DNI o número de socio
- Estado activo/inactivo

### Membresías
- Creación de membresías por período (mensual, anual, personalizado)
- Asignación de múltiples actividades por membresía
- Precio congelado al momento de la contratación
- Control de solapamiento de fechas
- Estados: activa, vencida, pago pendiente
- Visualización del saldo pendiente en tiempo real

### Pagos
- Registro de pagos parciales y totales
- Múltiples métodos de pago: efectivo, tarjeta, transferencia
- Generación automática de comprobante de pago
- Historial de pagos por socio y por período
- Estadísticas de recaudación
- Detección automática de socios morosos

### Actividades
- Alta y gestión de actividades (natación, gimnasia, etc.)
- Precio por actividad configurable
- Cuota base obligatoria separada de actividades opcionales

### Asistencias
- Registro de ingreso de socios
- Historial de asistencias por socio

### Usuarios y Roles
- Tres niveles de acceso: **Superadmin**, **Admin**, **Recepcionista**
- Control de permisos por rol
- Registro de quién procesó cada operación

### Auditoría
- Log completo de todas las operaciones del sistema
- Registro de usuario, fecha, hora y datos modificados

### Backup
- Creación manual de backup de la base de datos
- Descarga del archivo de backup
- Restauración desde archivo de backup

---

## Funciones a Futuro

### Control de Acceso Físico
- **Tarjeta RFID / QR por socio**: lectura en molinete o torniquete de entrada
- **App móvil para socios**: check-in desde el celular con código QR
- **Control de acceso por huella dactilar**
- Alertas automáticas si un socio moroso intenta ingresar

### Comunicación con Socios
- **Notificaciones por WhatsApp**: vencimiento de membresía, cuotas impagas, novedades del club
- **Envío de emails automáticos**: comprobante de pago, recordatorios de vencimiento
- **SMS**: alertas críticas (deuda vencida, suspensión)
- **Portal del socio**: acceso web para que el socio vea su estado de cuenta y descargue comprobantes

### Pagos Digitales
- **Integración con Mercado Pago**: pago de cuotas online desde el portal del socio
- **Débito automático**: cobro mensual automático a tarjeta o CBU
- **Facturación electrónica AFIP**: generación de facturas A y B

### Gestión Avanzada
- **Renovación automática de membresías**
- **Descuentos y promociones**: grupos familiares, descuentos por pago anticipado
- **Clases y turnos**: reserva de clases con cupo limitado
- **Profesores e instructores**: gestión de personal y asignación a actividades
- **Inventario**: control de equipamiento del club
- **Caja diaria**: cierre de caja con resumen de ingresos por método de pago

### Reportes y Estadísticas
- **Dashboard ejecutivo**: gráficos de socios activos, recaudación mensual, morosos
- **Reporte de retención**: socios que no renuevan
- **Exportación a Excel/PDF** de cualquier listado
- **Comparativa mensual y anual** de ingresos

### Infraestructura
- **Backups automáticos programados**: backup diario sin intervención manual
- **Notificación de backup exitoso/fallido** por email
- **App móvil para administrativos**: gestión desde celular

---

## Tecnologías Utilizadas

| Componente | Tecnología |
|---|---|
| Backend | .NET 8 — C# |
| Base de datos | Microsoft SQL Server 2022 |
| Frontend | Next.js 14 — React |
| Autenticación | JWT |
| Infraestructura | VPS con EasyPanel + Docker |
| API | RESTful con Swagger |

---

## Accesos del Sistema

| Rol | Permisos |
|---|---|
| Superadmin | Acceso total, configuración del sistema, backup |
| Admin | Gestión de socios, membresías, pagos y actividades |
| Recepcionista | Registro de asistencias y consulta de socios |

---

*Sistema desarrollado a medida — versión 1.0*
