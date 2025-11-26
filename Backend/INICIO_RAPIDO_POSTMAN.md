# Inicio Rápido con Postman (5 minutos)

## Opción Rápida: Importar la Colección Pre-configurada

Si no quieres configurar todo manualmente, importa los archivos que ya están listos:

### Paso 1: Descargar Postman
- Ve a [https://www.postman.com/downloads/](https://www.postman.com/downloads/)
- Descarga e instala Postman
- Abre Postman (no necesitas cuenta)

### Paso 2: Desactivar Verificación SSL

1. Haz clic en el ícono de **engranaje** (Settings) arriba a la derecha
2. Busca **"SSL certificate verification"**
3. **Desactívalo (OFF)**
4. Cierra la ventana

### Paso 3: Importar el Entorno (Environment)

1. Haz clic en **"Import"** (arriba a la izquierda)
2. Arrastra el archivo: `Local_Development.postman_environment.json`
3. Haz clic en **"Import"**
4. En el dropdown superior derecho, selecciona **"Local Development"**

### Paso 4: Importar la Colección

1. Haz clic en **"Import"** nuevamente
2. Arrastra el archivo: `Sistema_Gestion_Club.postman_collection.json`
3. Haz clic en **"Import"**
4. Verás la colección **"Sistema Gestión Club"** en el panel izquierdo

### Paso 5: Iniciar el Backend

Antes de probar, asegúrate de que el backend esté corriendo:

```bash
cd Backend/API
dotnet run
```

Espera a ver: `Now listening on: https://localhost:5000`

### Paso 6: Hacer tu Primera Petición

1. Expande la colección **"Sistema Gestión Club"**
2. Expande la carpeta **"Auth"**
3. Haz clic en **"Login - Admin"**
4. Haz clic en el botón azul **"Send"**

¡Listo! Deberías ver la respuesta con el token:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": 1,
    "nombreUsuario": "admin",
    "rol": "admin",
    ...
  }
}
```

### Paso 7: Probar Otros Endpoints

El token se guardó automáticamente. Ahora prueba:

1. **System → Health Check** (para verificar que el servidor está activo)
2. **Auth → Obtener Usuario Actual** (para ver tus datos)
3. **Socios → Obtener Todos los Socios** (para ver la lista de socios)
4. **Socios → Crear Socio** (para crear un nuevo socio)

---

## ¿Qué Incluye la Colección?

### Carpeta: System
- ✅ **Health Check** - Verificar que el servidor está activo

### Carpeta: Auth
- ✅ **Login - Admin** - Iniciar sesión como admin (guarda el token automáticamente)
- ✅ **Login - Recepcionista** - Iniciar sesión como recepcionista
- ✅ **Obtener Usuario Actual** - Ver tus datos

### Carpeta: Socios
- ✅ **Obtener Todos los Socios** - Listar socios (con paginación y filtros)
- ✅ **Obtener Socio por ID** - Ver detalles de un socio
- ✅ **Obtener Socio por Número** - Buscar socio por número
- ✅ **Crear Socio** - Agregar nuevo socio (solo admin)
- ✅ **Actualizar Socio** - Modificar datos de un socio (solo admin)
- ✅ **Desactivar Socio** - Dar de baja a un socio (solo admin)
- ✅ **Obtener Total de Socios** - Estadística de total de socios

---

## Características Especiales

### 🔒 Autenticación Automática
Todas las peticiones que requieren autenticación ya están configuradas con `{{token}}`. Solo haz login y todo funciona.

### 🧪 Tests Automáticos
Cada petición incluye tests que se ejecutan automáticamente:
- Verifican el código de respuesta
- Validan que la respuesta tenga los campos esperados
- Guardan datos útiles (como el token o el ID del último socio creado)

### 📝 Variables Dinámicas
La colección usa variables para facilitar las pruebas:
- `{{base_url}}` - URL base del servidor (https://localhost:5000)
- `{{token}}` - Token de autenticación (se guarda automáticamente al hacer login)
- `{{ultimo_socio_id}}` - ID del último socio creado (útil para actualizar/desactivar)

### 📖 Documentación Incluida
Cada petición tiene una descripción de qué hace y qué permisos necesita.

---

## Flujo de Trabajo Recomendado

### Primera Vez
1. **Health Check** - Verificar que el servidor está activo
2. **Login - Admin** - Iniciar sesión
3. **Obtener Usuario Actual** - Confirmar que estás autenticado
4. **Obtener Todos los Socios** - Ver los socios existentes

### Crear y Probar un Socio
1. **Login - Admin** - Asegurarte de tener permisos
2. **Crear Socio** - El ID se guarda automáticamente en `{{ultimo_socio_id}}`
3. **Obtener Socio por ID** - Modificar la URL a `/api/Socios/{{ultimo_socio_id}}`
4. **Actualizar Socio** - Ya usa `{{ultimo_socio_id}}` automáticamente
5. **Desactivar Socio** - Ya usa `{{ultimo_socio_id}}` automáticamente

### Probar Permisos
1. **Login - Recepcionista** - Cambiar a usuario con permisos limitados
2. **Crear Socio** - Debería dar error 403 Forbidden (no tiene permisos)
3. **Obtener Todos los Socios** - Debería funcionar (tiene permiso de lectura)

---

## Personalizar las Peticiones

### Cambiar Parámetros de Búsqueda

En **"Obtener Todos los Socios"**:

1. Ve a la pestaña **"Params"**
2. Activa/desactiva los checkboxes según necesites:
   - `search` - Buscar por texto
   - `estaActivo` - Filtrar activos/inactivos
   - `page` - Número de página
   - `pageSize` - Cantidad por página

### Modificar Datos del Nuevo Socio

En **"Crear Socio"**:

1. Ve a la pestaña **"Body"**
2. Modifica el JSON con los datos que quieras:

```json
{
  "nombre": "Tu Nombre",
  "apellido": "Tu Apellido",
  "dni": "12345678",
  "email": "tu.email@example.com",
  "telefono": "555-1234",
  "direccion": "Tu Dirección",
  "fechaNacimiento": "1990-01-01",
  "numeroSocio": "SOC-0200"
}
```

---

## Ver los Tests Ejecutándose

Después de enviar una petición:

1. Ve a la parte inferior donde está la respuesta
2. Haz clic en la pestaña **"Test Results"**
3. Verás tests como:
   - ✅ Login exitoso
   - ✅ Respuesta tiene token
   - ✅ Respuesta tiene usuario

---

## Errores Comunes

### ❌ "Could not get any response"

**Problema**: El backend no está corriendo.

**Solución**:
```bash
cd Backend/API
dotnet run
```

### ❌ "401 Unauthorized"

**Problema**: El token expiró o no se guardó.

**Solución**:
1. Ejecuta **"Login - Admin"** nuevamente
2. Verifica que el entorno **"Local Development"** esté seleccionado arriba a la derecha

### ❌ "403 Forbidden"

**Problema**: Tu usuario no tiene permisos para esa acción.

**Solución**:
- Si iniciaste sesión como `recepcionista`, cambia a `admin`
- Ejecuta **"Login - Admin"** para obtener permisos completos

### ❌ "400 Bad Request"

**Problema**: El JSON tiene errores o faltan campos requeridos.

**Solución**:
1. Verifica que todos los campos requeridos estén presentes
2. Asegúrate de que el formato JSON sea correcto (usa el validador de Postman)
3. Revisa la respuesta de error para ver qué campo falta

---

## Exportar y Compartir

### Para guardar tus cambios:

1. Haz clic derecho en la colección → **"Export"**
2. Selecciona **"Collection v2.1"**
3. Guarda el archivo

### Para compartir con tu equipo:

1. Comparte los archivos:
   - `Sistema_Gestion_Club.postman_collection.json`
   - `Local_Development.postman_environment.json`
2. Tus compañeros solo deben importarlos en Postman

---

## Siguiente Paso

Si quieres aprender más sobre Postman y todos los detalles, lee la **Guía Completa**:

👉 `GUIA_POSTMAN.md`

---

¡Ya estás listo para probar toda la API del Sistema de Gestión de Club! 🚀
