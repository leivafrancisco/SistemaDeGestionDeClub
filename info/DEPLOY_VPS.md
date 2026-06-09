# Deploy en VPS con Docker

Este documento explica paso a paso cómo subir y correr el sistema en un servidor VPS usando Docker.

---

## ¿Qué es Docker y por qué lo usamos?

Docker empaqueta la aplicación junto con todo lo que necesita para correr (el runtime de .NET, las dependencias, la configuración) en un **contenedor**. Esto garantiza que el sistema funcione igual en cualquier servidor, sin importar qué sistema operativo tenga el VPS.

---

## Requisitos previos

En tu **máquina local** necesitás tener instalado:
- Docker Desktop
- Git

En el **VPS** necesitás tener instalado:
- Docker
- Git

---

## Flujo general

```
Tu computadora                    VPS (Servidor)
──────────────                    ──────────────
Código fuente
     │
     │  docker build
     ↓
Imagen Docker ──── docker push ──→ Docker Hub (nube)
                                        │
                                        │  docker pull
                                        ↓
                                   Imagen en el VPS
                                        │
                                        │  docker run
                                        ↓
                                   App corriendo 🟢
```

---

## Paso 1 — Construir la imagen en tu máquina local

Desde la raíz del proyecto (`SistemaDeGestionDeClub/`) ejecutá:

```bash
docker build -f Backend/Dockerfile -t sistema-gestion-club:latest .
```

**¿Qué hace este comando?**
- `docker build` — le dice a Docker que construya una imagen
- `-f Backend/Dockerfile` — le indica dónde está el archivo Dockerfile
- `-t sistema-gestion-club:latest` — le pone un nombre y etiqueta a la imagen
- `.` — el contexto de construcción es la carpeta actual

### ¿Qué hace el Dockerfile internamente?

El `Dockerfile` tiene 3 etapas:

```dockerfile
# Etapa 1: imagen base liviana solo con el runtime de .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Etapa 2: imagen con el SDK completo para compilar el código
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  COPY [archivos del proyecto]
  RUN dotnet restore      ← descarga las dependencias NuGet
  RUN dotnet publish      ← compila y genera los binarios

# Etapa 3: copia solo los binarios compilados a la imagen liviana
FROM base AS final
  COPY --from=build /app/publish .
  ENTRYPOINT ["dotnet", "SistemaDeGestionDeClub.API.dll"]
```

Resultado: una imagen pequeña que solo tiene lo necesario para ejecutar la API.

---

## Paso 2 — Subir la imagen a Docker Hub

Docker Hub es el repositorio donde guardás tu imagen para que el VPS pueda descargarla.

```bash
# Iniciar sesión en Docker Hub
docker login

# Etiquetar la imagen con tu usuario de Docker Hub
docker tag sistema-gestion-club:latest tuusuario/sistema-gestion-club:latest

# Subir la imagen
docker push tuusuario/sistema-gestion-club:latest
```

---

## Paso 3 — Conectarse al VPS

```bash
ssh usuario@IP_DEL_VPS
```

---

## Paso 4 — Descargar y correr la imagen en el VPS

Una vez conectado al VPS:

```bash
# Descargar la imagen desde Docker Hub
docker pull tuusuario/sistema-gestion-club:latest

# Correr el contenedor
docker run -d \
  --name sistema-gestion-club \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=IP_DEL_VPS;Database=gestion_club;User Id=sa;Password=TuPassword123!;TrustServerCertificate=True;" \
  -e Jwt__Key="TuClaveSecretaSuperSeguraDeAlMenos32Caracteres!" \
  -e Jwt__Issuer="SistemaDeGestionDeClub" \
  -e Jwt__Audience="SistemaDeGestionDeClub" \
  tuusuario/sistema-gestion-club:latest
```

**¿Qué hace este comando?**

| Parámetro | Significado |
|-----------|-------------|
| `-d` | Corre en segundo plano (detached) |
| `--name` | Le pone un nombre al contenedor |
| `-p 8080:8080` | Expone el puerto 8080 del contenedor hacia el servidor |
| `-e` | Variables de entorno (reemplazan el appsettings.json) |

---

## Paso 5 — Verificar que está corriendo

```bash
# Ver los contenedores activos
docker ps

# Ver los logs de la aplicación
docker logs sistema-gestion-club

# Probar que responde
curl http://localhost:8080/swagger
```

---

## Comandos útiles para el día a día

```bash
# Detener el contenedor
docker stop sistema-gestion-club

# Reiniciar el contenedor
docker restart sistema-gestion-club

# Ver logs en tiempo real
docker logs -f sistema-gestion-club

# Actualizar a una nueva versión
docker pull tuusuario/sistema-gestion-club:latest
docker stop sistema-gestion-club
docker rm sistema-gestion-club
docker run -d ... (mismo comando del Paso 4)
```

---

## Resumen de comandos en orden

```bash
# --- EN TU MÁQUINA LOCAL ---
docker build -f Backend/Dockerfile -t sistema-gestion-club:latest .
docker tag sistema-gestion-club:latest tuusuario/sistema-gestion-club:latest
docker push tuusuario/sistema-gestion-club:latest

# --- EN EL VPS ---
ssh usuario@IP_DEL_VPS
docker pull tuusuario/sistema-gestion-club:latest
docker run -d -p 8080:8080 --name sistema-gestion-club tuusuario/sistema-gestion-club:latest
```
