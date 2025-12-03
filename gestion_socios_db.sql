USE gestion_socios;
GO

-- =============================================
-- 1. TABLAS BASE (Sin cambios mayores)
-- =============================================

CREATE TABLE roles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE personas (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    dni VARCHAR(20) UNIQUE,
    fecha_nacimiento DATE,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL
);

CREATE TABLE usuarios (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_persona INT NOT NULL UNIQUE,
    id_rol INT NOT NULL,
    nombre_usuario VARCHAR(50) NOT NULL UNIQUE,
    contrasena_hash VARCHAR(255) NOT NULL,
    esta_activo BIT NOT NULL DEFAULT 1,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    FOREIGN KEY (id_persona) REFERENCES personas(id),
    FOREIGN KEY (id_rol) REFERENCES roles(id)
);

CREATE TABLE socios (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_persona INT NOT NULL UNIQUE,
    numero_socio VARCHAR(20) NOT NULL UNIQUE,
    esta_activo BIT NOT NULL DEFAULT 1,
    fecha_alta DATE NOT NULL DEFAULT GETDATE(),
    fecha_baja DATE NULL,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    FOREIGN KEY (id_persona) REFERENCES personas(id)
);

CREATE TABLE metodos_pago (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE actividades (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(500),
    precio_actual DECIMAL(10, 2) NOT NULL, -- Renombrado para claridad
    es_cuota_base BIT NOT NULL DEFAULT 0,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL
);

-- =============================================
-- 2. TABLAS MODIFICADAS (Lógica de Negocio)
-- =============================================

-- Tabla membresias
-- CAMBIOS: Se eliminan periodo_anio/mes. Se agrega costo_total y estado.
CREATE TABLE membresias (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_socio INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    
    -- Nuevo: Guardamos cuántos meses dura para referencia rápida
    cantidad_meses DECIMAL(4,1) NOT NULL DEFAULT 1, 
    
    -- Nuevo: El valor final calculado ($250.000 en tu ejemplo)
    costo_total DECIMAL(12, 2) NOT NULL,
    
    -- Nuevo: Para saber si ya pagó todo sin tener que sumar la tabla pagos cada vez
    estado_pago VARCHAR(20) DEFAULT 'PENDIENTE' CHECK (estado_pago IN ('PENDIENTE', 'PARCIAL', 'PAGADO', 'VENCIDO')),
    
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    FOREIGN KEY (id_socio) REFERENCES socios(id)
);

-- Tabla membresia_actividades
-- CAMBIOS: precio_al_momento se mantiene pero representa el costo MENSUAL unitario
CREATE TABLE membresia_actividades (
    id_membresia INT NOT NULL,
    id_actividad INT NOT NULL,
    
    -- Este es el precio de la actividad por MES al momento de contratar ($30.000 o $20.000)
    precio_mensual_congelado DECIMAL(10, 2) NOT NULL, 
    
    PRIMARY KEY (id_membresia, id_actividad),
    FOREIGN KEY (id_membresia) REFERENCES membresias(id) ON DELETE CASCADE,
    FOREIGN KEY (id_actividad) REFERENCES actividades(id) ON DELETE CASCADE
);

-- Tabla pagos
-- CAMBIOS: Sin cambios estructurales, pero lógica: Se suman los montos para cubrir membresias.costo_total
CREATE TABLE pagos (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_membresia INT NOT NULL,
    id_metodo_pago INT NOT NULL,
    id_usuario_procesa INT NULL,
    monto DECIMAL(12, 2) NOT NULL, -- Aumenté precisión por si hay inflación
    fecha_pago DATE NOT NULL DEFAULT GETDATE(),
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    FOREIGN KEY (id_membresia) REFERENCES membresias(id),
    FOREIGN KEY (id_metodo_pago) REFERENCES metodos_pago(id),
    FOREIGN KEY (id_usuario_procesa) REFERENCES usuarios(id)
);

CREATE TABLE asistencias (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_socio INT NOT NULL,
    fecha_hora_ingreso DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    FOREIGN KEY (id_socio) REFERENCES socios(id)
);

-- =============================================
-- 3. ÍNDICES (Actualizados)
-- =============================================

CREATE INDEX IX_Personas_Email ON personas(email);
CREATE INDEX IX_Personas_DNI ON personas(dni);
CREATE INDEX IX_Socios_NumeroSocio ON socios(numero_socio);
CREATE INDEX IX_Socios_Persona ON socios(id_persona);
-- Índice actualizado: Buscamos membresías por rango de fechas ahora
CREATE INDEX IX_Membresias_SocioFecha ON membresias(id_socio, fecha_inicio, fecha_fin);
CREATE INDEX IX_Pagos_Membresia ON pagos(id_membresia);
CREATE INDEX IX_Pagos_Fecha ON pagos(fecha_pago);
CREATE INDEX IX_Asistencias_SocioFecha ON asistencias(id_socio, fecha_hora_ingreso DESC);

-- =============================================
-- 4. INSERTAR DATOS INICIALES
-- =============================================

INSERT INTO roles (nombre) VALUES ('superadmin'), ('admin'), ('recepcionista');
INSERT INTO metodos_pago (nombre) VALUES ('Efectivo'), ('Tarjeta'), ('Transferencia');
