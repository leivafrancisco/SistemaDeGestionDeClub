# Anexo B - Manual de Instalación y Configuración del Sistema de Gestión de Socios del Club

---

## Introducción

El propósito de este documento es describir la configuración e instalación del Sistema de Gestión de Socios del Club. El sistema es una API RESTful desarrollada en **.NET 8** que se apoya en una base de datos **SQL Server** y se despliega mediante **Docker**, lo que facilita su instalación en cualquier servidor compatible con contenedores.

---

## Objetivo de este manual

El objetivo es indicar los pasos y procedimientos a realizar para llevar a cabo la configuración, instalación y puesta en marcha del sistema aquí presentado, desde la preparación del servidor hasta la verificación de su correcto funcionamiento.

---

## Dirigido a

Este manual está dirigido al **usuario administrador de sistemas** de la organización, así como a todo perfil técnico encargado de la instalación, configuración y mantenimiento del mismo.

---

## Lo que deben conocer

Los conocimientos mínimos que deben tener las personas que operarán el sistema y deberán utilizar este manual son:

**Administrador de sistemas**: debe tener conocimientos profundos de la estructura y funcionamiento de la organización. Además es deseable que conozca aspectos fundamentales del sistema operativo del servidor (Linux o Windows Server), manejo de bases de datos mediante el lenguaje SQL, administración de contenedores Docker, conceptos de redes y direccionamiento IP, y manejo de archivos de configuración en formato JSON.

---

## Especificaciones técnicas

Para la implementación del sistema se deberá contar con los siguientes requerimientos:

### Hardware (servidor)

- CPU de al menos 2 GHz (se recomiendan 4 núcleos o más).
- 8 GB de memoria RAM como mínimo (se recomiendan 16 GB).
- 100 GB de disco duro disponible como mínimo (considerando el espacio para la base de datos, los backups y los logs del sistema).
- Interfaz de red (Ethernet) con dirección IP estática asignada.
- Se recomienda contar con una UPS para evitar posibles problemas energéticos en el servidor.

### Hardware (equipos cliente)

- Cualquier computadora, notebook o dispositivo móvil con acceso a un navegador web moderno.
- Conectividad de red hacia el servidor donde está alojado el sistema.

### Software (servidor)

- **Sistema operativo**: Linux (Ubuntu 20.04 LTS o superior recomendado) o Windows Server 2019/2022.
- **Docker Engine** 20.10 o superior.
- **Docker Compose** v2.0 o superior.
- Conexión a Internet para descargar las imágenes Docker durante la primera instalación (opcional si se trabaja con imágenes locales).

### Software (equipos cliente)

- Navegador web moderno: Google Chrome, Mozilla Firefox, Microsoft Edge o Safari (versiones actuales).
- No se requiere ninguna instalación adicional en los equipos cliente.

---

## Instalación y configuración del sistema

Para llevar a cabo la instalación del sistema, la organización debe contar con una red de comunicaciones bien definida y con la dirección IP del servidor asignada de forma **estática**, de manera que los clientes puedan acceder al sistema de forma consistente.

Lo primero que se debe realizar es verificar el cumplimiento de los requerimientos mínimos de hardware y software descritos con anterioridad en este documento.

---

### Paso 1 — Instalación de Docker en el servidor

El sistema se distribuye y ejecuta mediante contenedores Docker. Es necesario instalar Docker Engine y Docker Compose en el servidor antes de continuar.

**En Linux (Ubuntu/Debian):**

```bash
# Actualizar repositorios
sudo apt-get update

# Instalar dependencias
sudo apt-get install -y ca-certificates curl gnupg

# Agregar la clave GPG oficial de Docker
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | \
  sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Agregar el repositorio de Docker
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Instalar Docker Engine y Docker Compose
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Verificar la instalación
docker --version
docker compose version
```

**En Windows Server:**

Descargar e instalar **Docker Desktop for Windows** desde el sitio oficial de Docker. Asegurarse de habilitar la opción "Use WSL 2 based engine" durante la instalación.

---

### Paso 2 — Obtener el código fuente del sistema

Copiar la carpeta del proyecto al servidor. Se puede hacer de dos formas:

**Opción A — Mediante Git (recomendada):**

```bash
# Instalar Git si no está instalado
sudo apt-get install -y git

# Clonar el repositorio
git clone https://github.com/leivafrancisco/SistemaDeGestionDeClub.git

# Ingresar al directorio del proyecto
cd SistemaDeGestionDeClub
```

**Opción B — Copiando la carpeta manualmente:**

Copiar la carpeta completa del proyecto al servidor (por ejemplo mediante SFTP o un pendrive) y ubicarla en un directorio protegido, como `/opt/sistema-club/`.

---

### Paso 3 — Configurar el archivo de ajustes del sistema

El archivo de configuración principal del sistema es:

```
Backend/API/appsettings.json
```

Este archivo controla la cadena de conexión a la base de datos y la clave secreta utilizada para la generación de tokens JWT. **Antes de iniciar el sistema es obligatorio editar este archivo** con un editor de texto (por ejemplo `nano` o `vim` en Linux, o el Bloc de notas en Windows).

**Contenido del archivo al momento de la entrega:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=gestion_club;User Id=sa;Password=TuPassword123!;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "TuClaveSecretaSuperSeguraDeAlMenos32Caracteres!",
    "Issuer": "SistemaDeGestionDeClub",
    "Audience": "SistemaDeGestionDeClub"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Parámetros que el administrador debe modificar obligatoriamente:**

| Parámetro                          | Descripción                                                                 | Ejemplo de valor seguro                          |
|-----------------------------------|-----------------------------------------------------------------------------|--------------------------------------------------|
| `ConnectionStrings.DefaultConnection` → `Password` | Contraseña del usuario `sa` de SQL Server. Debe coincidir con la definida al levantar el contenedor. | `MiContraseñaSegura2024!` |
| `Jwt.Key`                         | Clave secreta para firmar los tokens JWT. Debe tener **al menos 32 caracteres**. Cambiarla por una cadena aleatoria y segura. | `X9kL#mP2qR7vN4wZ!jH6tY3sA1bC8dE` |

> **Importante:** nunca dejar los valores por defecto del archivo en un entorno de producción. Una clave JWT débil o conocida públicamente compromete la seguridad de todo el sistema.

**Parámetro `Server` en la cadena de conexión:**

El valor `Server=sqlserver` hace referencia al nombre del servicio de base de datos definido en Docker Compose. Si se utiliza Docker Compose para el despliegue (paso siguiente), este valor **no debe modificarse**. Solo debe cambiarse si se utiliza una instancia de SQL Server externa (en otro servidor).

---

### Paso 4 — Construir y levantar los contenedores Docker

El sistema se compone de dos contenedores:

1. **sqlserver**: instancia de SQL Server 2022 que almacena los datos del sistema.
2. **api**: la aplicación .NET 8 que expone los endpoints REST.

#### 4.1 — Construir la imagen de la API

Desde la raíz del proyecto (donde se encuentra el archivo `Dockerfile`), ejecutar:

```bash
# Construir la imagen Docker de la API
docker build -f Backend/Dockerfile -t sistema-gestion-club-api:latest .
```

#### 4.2 — Crear la red interna de Docker

```bash
docker network create club-network
```

#### 4.3 — Levantar el contenedor de SQL Server

Reemplazar `TuPassword123!` con la misma contraseña que se configuró en `appsettings.json`:

```bash
docker run -d \
  --name sqlserver \
  --network club-network \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=TuPassword123!" \
  -p 1433:1433 \
  -v sqlserver-data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

> **Nota:** el volumen `sqlserver-data` garantiza que los datos persisten aunque el contenedor se reinicie o se elimine.

#### 4.4 — Levantar el contenedor de la API

```bash
docker run -d \
  --name sistema-club-api \
  --network club-network \
  -p 8080:8080 \
  sistema-gestion-club-api:latest
```

#### 4.5 — Verificar que los contenedores están en ejecución

```bash
docker ps
```

La salida debe mostrar ambos contenedores (`sqlserver` y `sistema-club-api`) con estado `Up`.

---

### Paso 5 — Inicializar la base de datos

Una vez que el contenedor de SQL Server esté en ejecución, se debe ejecutar el script de inicialización de la base de datos. Este script crea todas las tablas, índices, restricciones y los **datos iniciales** (roles, métodos de pago, actividad base y usuarios de prueba).

El script se encuentra en la raíz del proyecto:

```
gestion_socios_db.sql
```

**Ejecutar el script dentro del contenedor de SQL Server:**

```bash
# Copiar el script al contenedor
docker cp gestion_socios_db.sql sqlserver:/tmp/gestion_socios_db.sql

# Ejecutar el script con sqlcmd
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "TuPassword123!" \
  -i /tmp/gestion_socios_db.sql \
  -C
```

Si la ejecución fue exitosa, el sistema mostrará los comandos procesados sin errores.

**Verificar que la base de datos se creó correctamente:**

```bash
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "TuPassword123!" -C \
  -Q "USE gestion_club; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME;"
```

La salida debe listar las siguientes tablas:

```
TABLE_NAME
-----------
actividades
asistencias
auditoria
cuotas
membresia_actividades
membresias
metodos_pago
pagos
personas
roles
socios
usuarios
```

---

### Paso 6 — Verificar el funcionamiento del sistema

Una vez levantados los contenedores y ejecutado el script de base de datos, verificar que el sistema responde correctamente.

#### 6.1 — Endpoint de salud (health check)

Desde cualquier equipo con acceso a la red del servidor, abrir un navegador web y acceder a:

```
http://<IP_DEL_SERVIDOR>:8080/health
```

Reemplazar `<IP_DEL_SERVIDOR>` con la dirección IP estática del servidor. Si el sistema está funcionando correctamente, la respuesta será similar a:

```json
{
  "status": "healthy",
  "timestamp": "2025-01-01T10:00:00Z"
}
```

#### 6.2 — Interfaz Swagger (documentación interactiva de la API)

El sistema expone una interfaz de documentación interactiva accesible desde el navegador:

```
http://<IP_DEL_SERVIDOR>:8080
```

Desde esta interfaz es posible explorar y probar todos los endpoints de la API. Es especialmente útil durante la puesta en marcha para verificar que la autenticación y los módulos funcionan correctamente.

---

### Paso 7 — Usuarios iniciales del sistema

El script de inicialización `gestion_socios_db.sql` carga automáticamente tres usuarios de prueba con sus respectivos roles:

| Nombre de usuario | Contraseña   | Rol           |
|------------------|--------------|---------------|
| `superadmin`     | `super123`   | Superadmin    |
| `admin`          | `admin123`   | Admin         |
| `recepcionista`  | `recep123`   | Recepcionista |

> **Advertencia de seguridad:** estas contraseñas son únicamente para verificar el funcionamiento inicial del sistema. El administrador debe cambiarlas **inmediatamente** luego de la primera verificación, antes de habilitar el acceso a otros usuarios. Esto puede hacerse utilizando el endpoint `PUT /api/auth/perfil` autenticándose con cada usuario.

Para cambiar la contraseña del superadmin en el primer acceso:

1. Autenticarse con `POST /api/auth/login` usando `superadmin` / `super123`.
2. Copiar el token JWT de la respuesta.
3. Llamar a `PUT /api/auth/perfil` con el token en el encabezado `Authorization: Bearer <token>` y el cuerpo:

```json
{
  "contrasenaActual": "super123",
  "nuevaContrasena": "NuevaContraseñaSegura!"
}
```

---

### Paso 8 — Carga inicial de datos del club

El script de base de datos incluye los siguientes datos iniciales mínimos:

**Roles:**
- `superadmin`, `admin`, `recepcionista`

**Métodos de pago:**
- `Efectivo`, `Tarjeta`, `Transferencia`

**Actividades:**
- `Cuota base` (cuota fija mensual obligatoria para todos los socios, precio: $0.00)

El administrador deberá completar la carga inicial de datos propios del club a través de los endpoints del sistema:

1. **Actividades adicionales**: registrar las disciplinas ofrecidas por el club (natación, tenis, gym, etc.) con sus precios actuales mediante `POST /api/actividades` (requiere rol Superadmin).
2. **Usuarios del sistema**: crear los usuarios para el personal del club (admins y recepcionistas) mediante `POST /api/usuarios` (requiere rol Admin o Superadmin).
3. **Socios**: registrar los socios existentes del club mediante `POST /api/socios` (requiere rol Admin o Superadmin).

Esta carga puede realizarse a través de la interfaz Swagger o mediante la colección de Postman incluida en el repositorio (`Backend/Sistema_Gestion_Club.postman_collection.json`).

---

### Paso 9 — Configuración del firewall y la red

El sistema expone el puerto **8080** (HTTP) para el acceso de los clientes. El administrador debe asegurarse de que:

- El puerto **8080** esté abierto en el firewall del servidor para permitir el acceso desde los equipos cliente de la red interna.
- El puerto **1433** (SQL Server) esté **cerrado** hacia el exterior, ya que la base de datos solo debe ser accesible desde el contenedor de la API, no desde los clientes.
- Se asigne una **dirección IP estática** al servidor para garantizar que los clientes siempre puedan encontrarlo en la misma dirección.

**Configurar el firewall en Linux (UFW):**

```bash
# Permitir acceso al puerto de la API desde la red interna
sudo ufw allow from 192.168.1.0/24 to any port 8080

# Asegurarse de que el puerto de SQL Server NO esté expuesto al exterior
sudo ufw deny 1433

# Activar el firewall
sudo ufw enable
sudo ufw status
```

Reemplazar `192.168.1.0/24` con el rango de red interna de la organización.

> **Nota:** si se desea habilitar HTTPS (recomendado para producción), se deberá configurar un proxy inverso como **Nginx** o **Caddy** frente al contenedor de la API, gestionar el certificado SSL y redirigir el tráfico del puerto 443 al 8080 interno. Esta configuración queda a cargo del administrador de sistemas y depende de la infraestructura de cada organización.

---

### Paso 10 — Configurar el reinicio automático de los contenedores

Para garantizar que el sistema se reinicie automáticamente ante un reinicio del servidor, ejecutar los siguientes comandos para que Docker gestione el ciclo de vida de los contenedores:

```bash
# Configurar reinicio automático del contenedor de SQL Server
docker update --restart unless-stopped sqlserver

# Configurar reinicio automático del contenedor de la API
docker update --restart unless-stopped sistema-club-api
```

Verificar que el servicio Docker también esté configurado para iniciar automáticamente con el sistema operativo:

```bash
sudo systemctl enable docker
```

---

## Actualización del sistema

Para actualizar el sistema a una nueva versión:

1. Detener el contenedor de la API:
   ```bash
   docker stop sistema-club-api && docker rm sistema-club-api
   ```
2. Obtener la nueva versión del código fuente (con `git pull` o copiando los archivos nuevos).
3. Reconstruir la imagen Docker (Paso 4.1).
4. Levantar nuevamente el contenedor de la API (Paso 4.4).
5. Si la nueva versión incluye cambios en la base de datos, ejecutar el script de migración correspondiente (Paso 5).

> **Importante:** realizar siempre un **backup de la base de datos** antes de actualizar. Ver la sección de Backup del Manual de Usuarios del Sistema (Anexo A).

---

## Resolución de problemas frecuentes

| Síntoma                                      | Causa probable                                        | Solución                                                         |
|---------------------------------------------|-------------------------------------------------------|------------------------------------------------------------------|
| El endpoint `/health` no responde            | El contenedor de la API no está en ejecución          | Ejecutar `docker ps` y verificar el estado. Revisar `docker logs sistema-club-api`. |
| Error `Login failed for user 'sa'`           | Contraseña incorrecta en `appsettings.json`           | Verificar que la contraseña en el JSON coincide con la del contenedor SQL Server. |
| Error `Cannot open database 'gestion_club'`  | El script SQL no fue ejecutado                        | Ejecutar el script `gestion_socios_db.sql` (Paso 5).            |
| El token JWT no es aceptado                  | La clave `Jwt:Key` fue cambiada luego de emitir tokens| Los tokens emitidos con la clave anterior quedan inválidos. Los usuarios deben volver a iniciar sesión. |
| El contenedor de SQL Server no inicia        | La contraseña no cumple los requisitos de complejidad | SQL Server requiere contraseñas con mayúsculas, minúsculas, números y símbolos. |
| Los datos se pierden al reiniciar el servidor| El volumen de Docker no está configurado              | Verificar que el contenedor de SQL Server fue creado con `-v sqlserver-data:/var/opt/mssql`. |

---

## Estructura del proyecto

```
SistemaDeGestionDeClub/
├── Backend/
│   ├── API/                     # Capa de presentación (controladores REST)
│   │   ├── Controllers/         # 10 controladores de la API
│   │   ├── appsettings.json     # Archivo de configuración principal ← EDITAR ANTES DE INSTALAR
│   │   └── Program.cs           # Punto de entrada y configuración de servicios
│   ├── Application/             # Capa de aplicación (servicios y DTOs)
│   ├── Domain/                  # Capa de dominio (entidades)
│   ├── Infrastructure/          # Capa de infraestructura (acceso a datos)
│   ├── Dockerfile               # Instrucciones para construir la imagen Docker
│   └── Sistema_Gestion_Club.postman_collection.json  # Colección de pruebas Postman
├── gestion_socios_db.sql        # Script de inicialización de la base de datos ← EJECUTAR EN PASO 5
└── Anexo_A__Manual_de_Usuarios_del_sistema.md
```

---

## Glosario

| Término          | Definición                                                                                          |
|-----------------|-----------------------------------------------------------------------------------------------------|
| **Docker**       | Plataforma de contenedores que permite empaquetar y ejecutar aplicaciones de forma aislada.        |
| **Contenedor**   | Unidad de software que empaqueta el código y sus dependencias para ejecutarse de forma confiable.  |
| **Docker Compose**| Herramienta para definir y ejecutar múltiples contenedores Docker como un único servicio.        |
| **SQL Server**   | Sistema gestor de bases de datos relacional de Microsoft utilizado por el sistema.                  |
| **API REST**     | Interfaz de programación que permite la comunicación entre cliente y servidor mediante HTTP.        |
| **JWT**          | JSON Web Token — mecanismo estándar para la autenticación segura entre cliente y servidor.         |
| **Swagger**      | Herramienta de documentación interactiva que permite explorar y probar los endpoints de la API.    |
| **Firewall**     | Sistema de seguridad que controla el tráfico de red entrante y saliente del servidor.              |
| **Soft delete**  | Patrón de eliminación lógica: el registro se marca como inactivo pero no se borra físicamente.     |
| **Health check** | Endpoint de verificación que indica si el sistema está funcionando correctamente.                  |
| **Volumen Docker**| Mecanismo de persistencia de datos en Docker, independiente del ciclo de vida del contenedor.    |
