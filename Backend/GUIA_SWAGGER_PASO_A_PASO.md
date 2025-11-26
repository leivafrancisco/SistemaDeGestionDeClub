# Guía Completa de Swagger - Paso a Paso (Para Principiantes)

## ¿Qué es Swagger?

**Swagger** es una interfaz web automática que se genera desde tu código del backend. Es como un "manual interactivo" de tu API.

---

## Swagger vs Postman: ¿Cuál es la diferencia?

| Característica | Swagger | Postman |
|---------------|---------|---------|
| **¿Qué es?** | Interfaz web que se genera automáticamente | Aplicación externa que debes instalar |
| **Instalación** | ❌ No necesita instalación | ✅ Necesitas descargar e instalar |
| **Dónde está** | En tu navegador (viene con el backend) | Aplicación separada |
| **Documentación** | ✅ Se actualiza automáticamente con tu código | ❌ Tienes que mantenerla manualmente |
| **Configuración** | ❌ Cero configuración | ✅ Debes configurar cada petición |
| **Mejor para** | Principiantes, pruebas rápidas | Equipos, pruebas complejas, automatización |
| **Disponible cuando** | Solo cuando el backend está corriendo | Siempre (incluso si el backend está apagado) |
| **Compartir** | Solo necesitas compartir la URL | Necesitas exportar/importar archivos JSON |

---

## ¿Por qué Swagger es MÁS FÁCIL para empezar?

### 1. **Ya está instalado** - No descargas nada
   - Viene incluido en tu backend
   - Solo abres el navegador

### 2. **Cero configuración** - Todo funciona de inmediato
   - No tienes que crear colecciones
   - No tienes que configurar variables
   - No tienes que agregar el token manualmente en cada petición

### 3. **Se actualiza solo** - Siempre está sincronizado
   - Si agregas un nuevo endpoint en el código → Aparece automáticamente en Swagger
   - Si cambias un parámetro → Se actualiza solo

### 4. **Más visual** - Todo en un solo lugar
   - Ves todos los endpoints organizados por secciones
   - Ves qué parámetros necesita cada uno
   - Ves ejemplos de respuestas

---

## Paso a Paso: Cómo usar Swagger

### ✅ PASO 1: Iniciar el Backend

Abre una terminal en la carpeta del proyecto y ejecuta:

```bash
cd Backend/API
dotnet run
```

**Espera a ver este mensaje:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

✅ Cuando veas esto, el backend está listo.

---

### ✅ PASO 2: Abrir Swagger en el Navegador

1. Abre tu navegador (Chrome, Edge, Firefox, etc.)
2. Ve a: **https://localhost:5000**
3. Si te sale un aviso de seguridad:
   - **Chrome/Edge**: Haz clic en "Opciones avanzadas" → "Continuar a localhost (no seguro)"
   - **Firefox**: Haz clic en "Avanzado" → "Aceptar el riesgo y continuar"

✅ Deberías ver la interfaz de **Swagger UI**

---

### 🎨 PASO 3: Entender la Interfaz de Swagger

Verás algo así:

```
┌─────────────────────────────────────────────────────────┐
│  Sistema de Gestión de Club API - v1                   │
│  API RESTful para gestión de socios, membresías...     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Schemas ▼                                              │
│                                                         │
│  ▼ Auth                                                 │
│     POST   /api/Auth/login          Login de usuario   │
│     GET    /api/Auth/me             Obtener usuario... │
│                                                         │
│  ▼ Socios                                              │
│     GET    /api/Socios              Obtener todos...   │
│     POST   /api/Socios              Crear un nuevo...  │
│     GET    /api/Socios/{id}         Obtener un socio...│
│     PUT    /api/Socios/{id}         Actualizar un...   │
│     ...                                                │
│                                                         │
│  [Authorize] 🔓                                        │
└─────────────────────────────────────────────────────────┘
```

**Elementos importantes:**

- **Secciones verdes (Auth, Socios)**: Agrupa endpoints relacionados
- **Métodos HTTP con colores**:
  - 🟢 **GET** (verde) = Obtener datos
  - 🟡 **POST** (amarillo) = Crear datos
  - 🔵 **PUT** (azul) = Actualizar datos
  - 🔴 **DELETE** (rojo) = Eliminar datos
- **Botón "Authorize" arriba a la derecha**: Para poner tu token
- **Candado 🔒**: Significa que ese endpoint necesita autenticación

---

### 🔐 PASO 4: Hacer Login (Obtener el Token)

#### 4.1 Abrir el endpoint de Login

1. Busca la sección **"Auth"**
2. Haz clic en: **`POST /api/Auth/login`**
3. Se despliega mostrando detalles
4. Haz clic en el botón **"Try it out"** (arriba a la derecha del endpoint)

**Antes de "Try it out":**
```
POST /api/Auth/login
Login de usuario
```

**Después de "Try it out":**
```
POST /api/Auth/login
Login de usuario

Request body *   [Campo de texto editable]
Example Value | Schema

[Execute]  [Cancel]
```

#### 4.2 Ingresar las credenciales

Verás un cuadro de texto con un JSON de ejemplo:

```json
{
  "nombreUsuario": "string",
  "password": "string"
}
```

**Bórralo todo** y escribe:

```json
{
  "nombreUsuario": "admin",
  "password": "admin123"
}
```

#### 4.3 Ejecutar la petición

1. Haz clic en el botón azul grande: **"Execute"**
2. Espera 1-2 segundos
3. Baja en la página y verás la sección **"Responses"**

#### 4.4 Ver la respuesta

En la sección "Responses" verás:

```
Server response
Code    Details

200     Response body
        Download

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEiLCJyb2xlIjoiYWRtaW4iLCJuYmYiOjE3MzE3NjgwMDAsImV4cCI6MTczMTc3MTYwMCwiaWF0IjoxNzMxNzY4MDAwfQ.abc123...",
  "usuario": {
    "id": 1,
    "nombreUsuario": "admin",
    "nombre": "Administrador",
    "apellido": "Sistema",
    "email": "admin@club.com",
    "rol": "admin"
  }
}
```

✅ **Código 200** = Todo salió bien

#### 4.5 Copiar el Token

1. En la respuesta, busca el campo `"token"`
2. **Copia SOLO el valor del token** (el texto largo que empieza con `eyJ...`)
   - NO copies las comillas
   - NO copies la palabra "token"
   - Solo copia: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

**Ejemplo de qué copiar:**
```
✅ CORRECTO: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEi...
❌ INCORRECTO: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEi..."
❌ INCORRECTO: token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 🔓 PASO 5: Autorizar tu sesión (Poner el Token)

#### 5.1 Abrir el diálogo de autorización

1. Sube al inicio de la página de Swagger
2. Busca el botón **"Authorize"** (arriba a la derecha)
3. Haz clic en él

Verás una ventana emergente:

```
Available authorizations

Bearer (apiKey)
  Value: [____________]

  [Authorize]  [Close]
```

#### 5.2 Ingresar el token

1. En el campo "Value", escribe: `Bearer ` (con espacio al final)
2. Pega el token que copiaste
3. Debería quedar así:

```
Value: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEi...
```

**MUY IMPORTANTE**: Debe tener la palabra `Bearer` seguida de un espacio y luego el token.

#### 5.3 Autorizar

1. Haz clic en el botón **"Authorize"**
2. Verás que el candado cambia de 🔓 a 🔒
3. Haz clic en **"Close"**

✅ **¡Ya estás autenticado!** Ahora puedes usar todos los endpoints protegidos.

---

### 📋 PASO 6: Probar Endpoints Protegidos

#### Ejemplo 1: Obtener Todos los Socios

1. Busca la sección **"Socios"**
2. Haz clic en: **`GET /api/Socios`**
3. Haz clic en **"Try it out"**
4. Verás varios parámetros opcionales:

```
Parameters
  search    string    Buscar por nombre, apellido...
            [______]

  estaActivo    boolean    Filtrar por activos
            [ ]

  page    integer    Número de página (default: 1)
            [1]

  pageSize    integer    Tamaño de página (default: 20)
            [20]
```

5. **Opción A - Sin filtros**: Deja todo como está y haz clic en **"Execute"**

6. **Opción B - Con filtros**:
   - En `search` escribe: `juan`
   - En `estaActivo` marca: `true`
   - En `pageSize` escribe: `5`
   - Haz clic en **"Execute"**

7. Verás la respuesta con la lista de socios:

```json
[
  {
    "id": 1,
    "personaId": 1,
    "numeroSocio": "SOC-0001",
    "fechaIngreso": "2024-01-15T00:00:00",
    "estaActivo": true,
    "nombre": "Juan",
    "apellido": "Pérez",
    "dni": "12345678",
    "email": "juan.perez@email.com",
    "telefono": "555-1234",
    "fechaNacimiento": "1990-05-15T00:00:00"
  }
]
```

✅ **Código 200** = Éxito

---

#### Ejemplo 2: Obtener Socio por ID

1. Busca: **`GET /api/Socios/{id}`**
2. Haz clic en **"Try it out"**
3. Verás un parámetro:

```
Parameters
  id *    integer (path)
          [___]
```

4. Escribe el ID del socio (ejemplo: `1`)
5. Haz clic en **"Execute"**
6. Verás los detalles de ese socio específico

---

#### Ejemplo 3: Crear un Nuevo Socio

1. Busca: **`POST /api/Socios`**
2. Haz clic en **"Try it out"**
3. Verás un cuadro de texto con el JSON de ejemplo:

```json
{
  "nombre": "string",
  "apellido": "string",
  "dni": "string",
  "email": "string",
  "telefono": "string",
  "direccion": "string",
  "fechaNacimiento": "2025-11-16T15:41:43.012Z",
  "numeroSocio": "string"
}
```

4. **Bórralo todo** y escribe datos reales:

```json
{
  "nombre": "María",
  "apellido": "López",
  "dni": "23456789",
  "email": "maria.lopez@email.com",
  "telefono": "555-5555",
  "direccion": "Calle Principal 456",
  "fechaNacimiento": "1992-08-10",
  "numeroSocio": "SOC-0050"
}
```

5. Haz clic en **"Execute"**
6. Si todo salió bien, verás:

```
Server response
Code    Details

201     Created
```

Y la respuesta mostrará el socio creado con su nuevo ID:

```json
{
  "id": 5,
  "personaId": 5,
  "numeroSocio": "SOC-0050",
  "fechaIngreso": "2024-11-16T15:45:00",
  "estaActivo": true,
  "nombre": "María",
  "apellido": "López",
  "dni": "23456789",
  "email": "maria.lopez@email.com",
  "telefono": "555-5555",
  "fechaNacimiento": "1992-08-10T00:00:00"
}
```

✅ **Código 201** = Creado exitosamente

---

#### Ejemplo 4: Actualizar un Socio

1. Busca: **`PUT /api/Socios/{id}`**
2. Haz clic en **"Try it out"**
3. Ingresa el ID del socio que quieres actualizar (ejemplo: `5`)
4. Modifica el JSON con los nuevos datos:

```json
{
  "nombre": "María Elena",
  "apellido": "López García",
  "dni": "23456789",
  "email": "maria.lopez.nueva@email.com",
  "telefono": "555-6666",
  "direccion": "Avenida Central 789",
  "fechaNacimiento": "1992-08-10"
}
```

5. Haz clic en **"Execute"**
6. Verás el socio actualizado

✅ **Código 200** = Actualizado exitosamente

---

#### Ejemplo 5: Desactivar un Socio

1. Busca: **`PUT /api/Socios/{id}/desactivar`**
2. Haz clic en **"Try it out"**
3. Ingresa el ID del socio (ejemplo: `5`)
4. Haz clic en **"Execute"**
5. Verás:

```json
{
  "message": "Socio desactivado exitosamente"
}
```

✅ **Código 200** = Desactivado exitosamente

---

### 🧪 PASO 7: Probar con Otro Usuario (Recepcionista)

Para ver cómo funcionan los permisos:

#### 7.1 Cerrar sesión actual

1. Haz clic en **"Authorize"** arriba a la derecha
2. Haz clic en **"Logout"**
3. Haz clic en **"Close"**

#### 7.2 Hacer login como recepcionista

1. Ve a: **`POST /api/Auth/login`**
2. **"Try it out"**
3. Ingresa:

```json
{
  "nombreUsuario": "recepcionista",
  "password": "recep123"
}
```

4. **"Execute"**
5. Copia el nuevo token

#### 7.3 Autorizar con el nuevo token

1. **"Authorize"**
2. Ingresa: `Bearer [nuevo_token]`
3. **"Authorize"** → **"Close"**

#### 7.4 Intentar crear un socio (Debería fallar)

1. Ve a: **`POST /api/Socios`**
2. **"Try it out"**
3. Ingresa cualquier JSON válido
4. **"Execute"**

Verás:

```
Server response
Code    Details

403     Forbidden
```

✅ Esto es correcto! El recepcionista NO tiene permisos para crear socios.

#### 7.5 Ver socios (Debería funcionar)

1. Ve a: **`GET /api/Socios`**
2. **"Try it out"**
3. **"Execute"**

✅ **Código 200** = El recepcionista SÍ puede ver socios.

---

### 📊 PASO 8: Entender los Códigos de Respuesta

Swagger te muestra diferentes códigos según el resultado:

| Código | Color | Significado | Ejemplo |
|--------|-------|-------------|---------|
| **200** | 🟢 Verde | Éxito | Datos obtenidos correctamente |
| **201** | 🟢 Verde | Creado | Socio creado exitosamente |
| **400** | 🔴 Rojo | Error de cliente | JSON mal formateado o datos inválidos |
| **401** | 🔴 Rojo | No autenticado | No tienes token o expiró |
| **403** | 🔴 Rojo | Sin permiso | Tu usuario no tiene acceso |
| **404** | 🟡 Amarillo | No encontrado | El socio con ese ID no existe |
| **500** | 🔴 Rojo | Error del servidor | Error en el backend |

---

### 🎯 PASO 9: Usar el Schema para ver la estructura

Swagger te muestra qué campos necesita cada endpoint:

1. En cualquier endpoint, busca la palabra **"Schema"** al lado de "Example Value"
2. Haz clic en **"Schema"**
3. Verás la estructura detallada:

```
CrearSocioDto {
  nombre*         string
  apellido*       string
  dni*            string
  email*          string
  telefono        string (nullable)
  direccion       string (nullable)
  fechaNacimiento*    string (date-time)
  numeroSocio*    string
}

* = required
```

Esto te dice:
- ✅ **Campos obligatorios** (con asterisco *)
- ⚪ **Campos opcionales** (nullable)
- 📝 **Tipo de dato** (string, integer, boolean, date-time)

---

### ⚡ PASO 10: Health Check (Verificar que todo funciona)

Este endpoint NO necesita autenticación:

1. Busca: **`GET /health`**
2. Haz clic en **"Try it out"**
3. Haz clic en **"Execute"**
4. Verás:

```json
{
  "status": "healthy",
  "timestamp": "2024-11-16T15:50:00.123Z"
}
```

✅ Si ves esto, el backend está funcionando correctamente.

---

## 🎓 Conceptos Clave de Swagger

### 1. **Request Body (Cuerpo de la petición)**
   - Es el JSON que envías al servidor
   - Solo lo usan POST y PUT
   - Ejemplo: Datos del nuevo socio a crear

### 2. **Parameters (Parámetros)**
   - **Path parameters**: Van en la URL (ejemplo: `/api/Socios/{id}`)
   - **Query parameters**: Van después del `?` (ejemplo: `?search=juan&page=1`)
   - **Header parameters**: Van en las cabeceras (ejemplo: Authorization)

### 3. **Responses (Respuestas)**
   - **Response body**: El JSON que devuelve el servidor
   - **Response code**: El código HTTP (200, 201, 400, etc.)
   - **Response headers**: Información adicional (content-type, etc.)

---

## 🔄 Flujo de Trabajo Típico en Swagger

```
1. Iniciar el backend
   ↓
2. Abrir Swagger en el navegador
   ↓
3. Hacer Login (POST /api/Auth/login)
   ↓
4. Copiar el token
   ↓
5. Hacer clic en "Authorize"
   ↓
6. Pegar "Bearer [token]"
   ↓
7. Probar endpoints protegidos
   ↓
8. Si el token expira: Volver al paso 3
```

---

## ❌ Errores Comunes y Soluciones

### Error: "Failed to fetch"

**Problema**: El backend no está corriendo.

**Solución**:
```bash
cd Backend/API
dotnet run
```

---

### Error: "401 Unauthorized"

**Problema**: No has puesto el token o expiró.

**Solución**:
1. Haz login de nuevo
2. Copia el nuevo token
3. Authorize → `Bearer [token]`

---

### Error: "400 Bad Request"

**Problema**: El JSON tiene errores.

**Solución**:
1. Verifica que todos los campos requeridos (*) estén presentes
2. Verifica que las fechas tengan formato correcto: `YYYY-MM-DD`
3. Verifica que no haya comas de más en el JSON

**Ejemplo de JSON inválido:**
```json
{
  "nombre": "Juan",
  "apellido": "Pérez",  ← Coma de más
}
```

**Ejemplo de JSON válido:**
```json
{
  "nombre": "Juan",
  "apellido": "Pérez"
}
```

---

### Error: "403 Forbidden"

**Problema**: Tu usuario no tiene permisos.

**Solución**:
- Haz logout
- Haz login como `admin` en lugar de `recepcionista`
- Autoriza de nuevo con el token de admin

---

## 🆚 Cuándo usar Swagger vs Postman

### Usa **Swagger** cuando:
- ✅ Estés aprendiendo o empezando
- ✅ Quieras probar rápidamente un endpoint
- ✅ Necesites ver la documentación de la API
- ✅ Solo tengas acceso a un navegador

### Usa **Postman** cuando:
- ✅ Necesites guardar colecciones de peticiones
- ✅ Trabajes en equipo y quieran compartir peticiones
- ✅ Necesites automatizar pruebas
- ✅ Quieras crear flujos complejos con variables
- ✅ El backend esté en producción (no en tu computadora)

### Mejor opción: **Ambos**
- **Swagger** para desarrollo y pruebas rápidas
- **Postman** para pruebas más elaboradas y compartir con el equipo

---

## 💡 Ventajas de Swagger

1. **Cero instalación** - Solo necesitas el navegador
2. **Siempre actualizado** - Se genera automáticamente del código
3. **Documentación integrada** - Ves qué hace cada endpoint
4. **Validación visual** - Ves los tipos de datos esperados
5. **Pruebas instantáneas** - Try it out → Execute

---

## 📝 Resumen - Cheat Sheet

```bash
# 1. Iniciar backend
cd Backend/API
dotnet run

# 2. Abrir navegador
https://localhost:5000

# 3. Login
POST /api/Auth/login
{
  "nombreUsuario": "admin",
  "password": "admin123"
}

# 4. Copiar token y autorizar
Authorize → Bearer eyJhbGciOiJIUzI1NiIs...

# 5. Probar endpoints
GET /api/Socios → Ver todos
POST /api/Socios → Crear
PUT /api/Socios/1 → Actualizar
PUT /api/Socios/1/desactivar → Desactivar
```

---

¡Ya eres un experto en Swagger! 🚀 Ahora puedes probar tu API fácilmente sin instalar nada.
