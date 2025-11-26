# Guía Completa: Probar el Backend con Postman

## Paso 1: Instalar Postman

1. Ve a [https://www.postman.com/downloads/](https://www.postman.com/downloads/)
2. Descarga e instala Postman para Windows
3. Abre Postman (puedes usar la versión gratuita, no necesitas crear cuenta)

---

## Paso 2: Crear una Colección (Collection)

Las colecciones te ayudan a organizar todas las peticiones de tu API.

1. En Postman, haz clic en **"Collections"** en el panel izquierdo
2. Haz clic en el botón **"+"** o **"Create Collection"**
3. Nómbrala: **"Sistema Gestión Club"**
4. Haz clic derecho en la colección → **"Add folder"**
   - Crea una carpeta llamada **"Auth"**
   - Crea otra carpeta llamada **"Socios"**

---

## Paso 3: Configurar Variables de Entorno

Esto te permite cambiar la URL base fácilmente (por ejemplo, de localhost a producción).

1. Haz clic en **"Environments"** en el panel izquierdo
2. Haz clic en **"+"** para crear un nuevo entorno
3. Nómbralo: **"Local Development"**
4. Agrega estas variables:

| Variable | Initial Value | Current Value |
|----------|---------------|---------------|
| `base_url` | `https://localhost:5000` | `https://localhost:5000` |
| `token` | (déjalo vacío) | (déjalo vacío) |

5. Haz clic en **"Save"**
6. Selecciona el entorno **"Local Development"** en el dropdown superior derecho

---

## Paso 4: Probar el Login (POST /api/Auth/login)

### 4.1 Crear la petición

1. Haz clic derecho en la carpeta **"Auth"** → **"Add request"**
2. Nómbrala: **"Login - Admin"**
3. Configura:
   - **Método**: `POST`
   - **URL**: `{{base_url}}/api/Auth/login`

### 4.2 Configurar el Body

1. Ve a la pestaña **"Body"**
2. Selecciona **"raw"**
3. En el dropdown de la derecha, selecciona **"JSON"**
4. Ingresa este JSON:

```json
{
  "nombreUsuario": "admin",
  "password": "admin123"
}
```

### 4.3 Deshabilitar verificación SSL (solo para desarrollo local)

1. Ve a **"Settings"** (ícono de engranaje arriba a la derecha)
2. Busca **"SSL certificate verification"**
3. Desactívalo (OFF)
4. Cierra Settings

### 4.4 Enviar la petición

1. Haz clic en **"Send"**
2. Verás la respuesta en la parte inferior:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEiLCJyb2xlIjoiYWRtaW4iLCJuYmYiOjE3MzE3NjgwMDAsImV4cCI6MTczMTc3MTYwMCwiaWF0IjoxNzMxNzY4MDAwfQ...",
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

### 4.5 Guardar el Token Automáticamente (Opcional pero MUY ÚTIL)

Para no tener que copiar el token manualmente cada vez:

1. Ve a la pestaña **"Tests"** en la petición de Login
2. Ingresa este código:

```javascript
// Guardar el token en la variable de entorno
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.environment.set("token", jsonData.token);
    console.log("Token guardado:", jsonData.token);
}
```

3. Haz clic en **"Save"**
4. Vuelve a hacer clic en **"Send"**
5. Ahora el token se guarda automáticamente en la variable `{{token}}`

---

## Paso 5: Probar Obtener Usuario Actual (GET /api/Auth/me)

### 5.1 Crear la petición

1. Haz clic derecho en la carpeta **"Auth"** → **"Add request"**
2. Nómbrala: **"Obtener Usuario Actual"**
3. Configura:
   - **Método**: `GET`
   - **URL**: `{{base_url}}/api/Auth/me`

### 5.2 Configurar la Autenticación

1. Ve a la pestaña **"Authorization"**
2. En **"Type"**, selecciona **"Bearer Token"**
3. En el campo **"Token"**, ingresa: `{{token}}`

### 5.3 Enviar la petición

1. Haz clic en **"Send"**
2. Verás la información del usuario:

```json
{
  "id": 1,
  "nombreUsuario": "admin",
  "nombre": "Administrador",
  "apellido": "Sistema",
  "email": "admin@club.com",
  "rol": "admin"
}
```

---

## Paso 6: Probar Obtener Todos los Socios (GET /api/Socios)

### 6.1 Crear la petición

1. Haz clic derecho en la carpeta **"Socios"** → **"Add request"**
2. Nómbrala: **"Obtener Todos los Socios"**
3. Configura:
   - **Método**: `GET`
   - **URL**: `{{base_url}}/api/Socios`

### 6.2 Configurar la Autenticación

1. Ve a la pestaña **"Authorization"**
2. Selecciona **"Bearer Token"**
3. En el campo **"Token"**, ingresa: `{{token}}`

### 6.3 Agregar Parámetros de Query (Opcional)

1. Ve a la pestaña **"Params"**
2. Agrega estos parámetros (todos son opcionales):

| Key | Value | Descripción |
|-----|-------|-------------|
| `search` | `juan` | Buscar por nombre, apellido, email, dni |
| `estaActivo` | `true` | Filtrar por activos/inactivos |
| `page` | `1` | Número de página |
| `pageSize` | `10` | Cantidad por página |

### 6.4 Enviar la petición

1. Haz clic en **"Send"**
2. Verás la lista de socios:

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

---

## Paso 7: Probar Obtener Socio por ID (GET /api/Socios/{id})

### 7.1 Crear la petición

1. Haz clic derecho en la carpeta **"Socios"** → **"Add request"**
2. Nómbrala: **"Obtener Socio por ID"**
3. Configura:
   - **Método**: `GET`
   - **URL**: `{{base_url}}/api/Socios/1`

### 7.2 Configurar la Autenticación

1. Ve a la pestaña **"Authorization"**
2. Selecciona **"Bearer Token"**
3. Token: `{{token}}`

### 7.3 Enviar la petición

1. Haz clic en **"Send"**
2. Verás los detalles del socio con ID 1

---

## Paso 8: Probar Crear un Nuevo Socio (POST /api/Socios)

### 8.1 Crear la petición

1. Haz clic derecho en la carpeta **"Socios"** → **"Add request"**
2. Nómbrala: **"Crear Socio"**
3. Configura:
   - **Método**: `POST`
   - **URL**: `{{base_url}}/api/Socios`

### 8.2 Configurar la Autenticación

1. Ve a la pestaña **"Authorization"**
2. Selecciona **"Bearer Token"**
3. Token: `{{token}}`

### 8.3 Configurar el Body

1. Ve a la pestaña **"Body"**
2. Selecciona **"raw"** → **"JSON"**
3. Ingresa este JSON:

```json
{
  "nombre": "Carlos",
  "apellido": "González",
  "dni": "87654321",
  "email": "carlos.gonzalez@email.com",
  "telefono": "555-9876",
  "direccion": "Calle Falsa 123",
  "fechaNacimiento": "1985-03-20",
  "numeroSocio": "SOC-0100"
}
```

### 8.4 Enviar la petición

1. Haz clic en **"Send"**
2. Si el socio se creó correctamente, verás:

```json
{
  "id": 10,
  "personaId": 10,
  "numeroSocio": "SOC-0100",
  "fechaIngreso": "2024-11-16T10:30:00",
  "estaActivo": true,
  "nombre": "Carlos",
  "apellido": "González",
  "dni": "87654321",
  "email": "carlos.gonzalez@email.com",
  "telefono": "555-9876",
  "fechaNacimiento": "1985-03-20T00:00:00"
}
```

---

## Paso 9: Probar Actualizar un Socio (PUT /api/Socios/{id})

### 9.1 Crear la petición

1. Haz clic derecho en la carpeta **"Socios"** → **"Add request"**
2. Nómbrala: **"Actualizar Socio"**
3. Configura:
   - **Método**: `PUT`
   - **URL**: `{{base_url}}/api/Socios/10`

### 9.2 Configurar la Autenticación

1. Authorization → **"Bearer Token"**
2. Token: `{{token}}`

### 9.3 Configurar el Body

1. Body → **"raw"** → **"JSON"**
2. Ingresa:

```json
{
  "nombre": "Carlos Andrés",
  "apellido": "González Méndez",
  "dni": "87654321",
  "email": "carlos.gonzalez.nuevo@email.com",
  "telefono": "555-9999",
  "direccion": "Avenida Siempre Viva 742",
  "fechaNacimiento": "1985-03-20"
}
```

### 9.4 Enviar la petición

1. Haz clic en **"Send"**
2. Verás el socio actualizado

---

## Paso 10: Probar Desactivar un Socio (PUT /api/Socios/{id}/desactivar)

### 10.1 Crear la petición

1. Haz clic derecho en la carpeta **"Socios"** → **"Add request"**
2. Nómbrala: **"Desactivar Socio"**
3. Configura:
   - **Método**: `PUT`
   - **URL**: `{{base_url}}/api/Socios/10/desactivar`

### 10.2 Configurar la Autenticación

1. Authorization → **"Bearer Token"**
2. Token: `{{token}}`

### 10.3 Enviar la petición

1. Haz clic en **"Send"**
2. Verás:

```json
{
  "message": "Socio desactivado exitosamente"
}
```

---

## Paso 11: Probar con Usuario Recepcionista

Para probar los permisos de rol:

### 11.1 Crear nueva petición de Login

1. Duplica la petición **"Login - Admin"**
2. Nómbrala: **"Login - Recepcionista"**
3. Cambia el body a:

```json
{
  "nombreUsuario": "recepcionista",
  "password": "recep123"
}
```

### 11.2 Probar Crear Socio (Debería Fallar)

1. Haz login como recepcionista
2. Intenta usar la petición **"Crear Socio"**
3. Deberías recibir un error **403 Forbidden**:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

Esto es correcto porque solo `admin` y `superadmin` pueden crear socios.

---

## Paso 12: Probar el Health Check

### 12.1 Crear la petición

1. Crea una nueva carpeta en la colección llamada **"System"**
2. Agrega una petición llamada **"Health Check"**
3. Configura:
   - **Método**: `GET`
   - **URL**: `{{base_url}}/health`
   - **NO necesita autenticación**

### 12.2 Enviar la petición

1. Haz clic en **"Send"**
2. Verás:

```json
{
  "status": "healthy",
  "timestamp": "2024-11-16T14:30:00.123Z"
}
```

---

## Trucos y Consejos

### 1. Usar Variables para IDs Dinámicos

En la pestaña **"Tests"** de una petición de crear socio:

```javascript
if (pm.response.code === 201) {
    var socio = pm.response.json();
    pm.environment.set("ultimo_socio_id", socio.id);
}
```

Luego puedes usar `{{ultimo_socio_id}}` en otras peticiones.

### 2. Ver el Token Decodificado

1. Copia el token
2. Ve a [https://jwt.io/](https://jwt.io/)
3. Pégalo para ver su contenido:

```json
{
  "unique_name": "1",
  "role": "admin",
  "nbf": 1731768000,
  "exp": 1731771600,
  "iat": 1731768000
}
```

### 3. Organizar Peticiones por Orden de Ejecución

En Postman puedes arrastrar las peticiones para organizarlas:

```
Sistema Gestión Club/
├── System/
│   └── Health Check
├── Auth/
│   ├── Login - Admin
│   ├── Login - Recepcionista
│   └── Obtener Usuario Actual
└── Socios/
    ├── Obtener Todos los Socios
    ├── Obtener Socio por ID
    ├── Obtener Socio por Número
    ├── Crear Socio
    ├── Actualizar Socio
    └── Desactivar Socio
```

### 4. Exportar e Importar la Colección

**Exportar:**
1. Haz clic derecho en la colección → **"Export"**
2. Selecciona **"Collection v2.1"**
3. Guarda el archivo JSON

**Importar:**
1. Haz clic en **"Import"** (arriba a la izquierda)
2. Arrastra el archivo JSON
3. Listo!

### 5. Crear Tests Automáticos

En la pestaña **"Tests"** de cualquier petición:

```javascript
// Verificar que la respuesta sea 200
pm.test("Status es 200", function () {
    pm.response.to.have.status(200);
});

// Verificar que la respuesta tenga un campo específico
pm.test("Tiene campo token", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('token');
});

// Verificar que la respuesta sea JSON
pm.test("Respuesta es JSON", function () {
    pm.response.to.be.json;
});
```

---

## Errores Comunes

### Error: "Could not get any response"

**Causa**: El backend no está corriendo o la URL es incorrecta.

**Solución**:
1. Verifica que el backend esté corriendo: `dotnet run`
2. Verifica que la URL sea correcta: `https://localhost:5000`
3. Desactiva SSL verification en Settings

### Error: "401 Unauthorized"

**Causa**: No estás enviando el token o el token expiró.

**Solución**:
1. Haz login nuevamente para obtener un nuevo token
2. Verifica que hayas configurado el Bearer Token en Authorization
3. Asegúrate de estar usando `{{token}}` correctamente

### Error: "403 Forbidden"

**Causa**: Tu usuario no tiene permisos para ese endpoint.

**Solución**:
1. Verifica que estés usando el usuario correcto (admin vs recepcionista)
2. Revisa los roles requeridos para ese endpoint

### Error: "400 Bad Request"

**Causa**: El JSON que estás enviando tiene errores o falta información.

**Solución**:
1. Verifica que el JSON esté bien formateado
2. Asegúrate de incluir todos los campos requeridos
3. Revisa que los tipos de datos sean correctos (strings, números, fechas)

---

## Resumen de Flujo Típico

1. **Iniciar el backend**: `dotnet run` en la carpeta `Backend/API`
2. **Hacer Login**: Enviar `POST /api/Auth/login` con credenciales
3. **El token se guarda automáticamente** (si configuraste el Test)
4. **Probar endpoints protegidos**: Todos usan `{{token}}` en Authorization
5. **Cuando el token expire**: Volver a hacer login

---

## Siguiente Paso

Ahora que sabes usar Postman, te recomiendo:

1. Crear todas las peticiones de la API
2. Guardar la colección para compartirla con tu equipo
3. Crear tests automáticos para verificar que todo funciona
4. Experimentar con diferentes parámetros y casos de error

¡Ya estás listo para probar tu API completamente con Postman!
