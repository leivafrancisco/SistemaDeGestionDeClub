IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'gestion_club')
BEGIN
    CREATE DATABASE gestion_club;
END
GO

USE gestion_club;
GO


-- Tabla: roles
CREATE TABLE roles (
    id INT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(50) NOT NULL,
    CONSTRAINT PK_roles PRIMARY KEY CLUSTERED (id),
    CONSTRAINT UQ_roles_nombre UNIQUE (nombre)
);
GO

-- Tabla: metodos_pago
CREATE TABLE metodos_pago (
    id INT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(50) NOT NULL,
    CONSTRAINT PK_metodos_pago PRIMARY KEY CLUSTERED (id),
    CONSTRAINT UQ_metodos_pago_nombre UNIQUE (nombre)
);
GO

-- Tabla: personas
CREATE TABLE personas (
    id INT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    dni VARCHAR(20) NULL,
    fecha_nacimiento DATE NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    CONSTRAINT PK_personas PRIMARY KEY CLUSTERED (id)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_personas_email ON personas(email) WHERE email IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UQ_personas_dni ON personas(dni) WHERE dni IS NOT NULL;
GO

-- Tabla: usuarios
CREATE TABLE usuarios (
    id INT IDENTITY(1,1) NOT NULL,
    id_persona INT NOT NULL,
    id_rol INT NOT NULL,
    nombre_usuario VARCHAR(50) NOT NULL,
    contrasena_hash VARCHAR(255) NOT NULL,
    esta_activo BIT NOT NULL DEFAULT 1,
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    CONSTRAINT PK_usuarios PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_usuarios_personas FOREIGN KEY (id_persona) REFERENCES personas(id),
    CONSTRAINT FK_usuarios_roles FOREIGN KEY (id_rol) REFERENCES roles(id)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_usuarios_nombre_usuario ON usuarios(nombre_usuario);
CREATE UNIQUE NONCLUSTERED INDEX UQ_usuarios_id_persona ON usuarios(id_persona);
GO

-- Tabla: socios
CREATE TABLE socios (
    id INT IDENTITY(1,1) NOT NULL,
    id_persona INT NOT NULL,
    numero_socio VARCHAR(20) NOT NULL,
    esta_activo BIT NOT NULL DEFAULT 1,
    fecha_alta DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    fecha_baja DATE NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    CONSTRAINT PK_socios PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_socios_personas FOREIGN KEY (id_persona) REFERENCES personas(id)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_socios_numero_socio ON socios(numero_socio);
CREATE UNIQUE NONCLUSTERED INDEX UQ_socios_id_persona ON socios(id_persona);
GO

-- Tabla: actividades
-- es_cuota_base = 1 indica la cuota fija obligatoria que siempre forma parte de la membresía.
-- es_cuota_base = 0 son actividades opcionales (ej: Gimnasio, Natación).
CREATE TABLE actividades (
    id INT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(500) NULL,
    precio_actual DECIMAL(10, 2) NOT NULL,
    es_cuota_base BIT NOT NULL DEFAULT 0,
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    CONSTRAINT PK_actividades PRIMARY KEY CLUSTERED (id)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_actividades_nombre ON actividades(nombre);
GO

-- Tabla: membresias
-- costo_total = SUM(precio_mensual_congelado) de todas las actividades asignadas.
-- estado: 'activa' | 'pago_pendiente' | 'vencida'
CREATE TABLE membresias (
    id INT IDENTITY(1,1) NOT NULL,
    id_socio INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    costo_total DECIMAL(12, 2) NOT NULL,
    estado VARCHAR(45) NOT NULL DEFAULT 'activa',
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    CONSTRAINT PK_membresias PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_membresias_socios FOREIGN KEY (id_socio) REFERENCES socios(id),
    CONSTRAINT CHK_membresias_fechas CHECK (fecha_fin > fecha_inicio)
);
GO

CREATE NONCLUSTERED INDEX IX_membresias_id_socio ON membresias(id_socio);
CREATE NONCLUSTERED INDEX IX_membresias_fechas ON membresias(fecha_inicio, fecha_fin);
GO

-- Tabla: membresia_actividades
-- Almacena el precio congelado al momento de asignar la actividad.
-- Incluye tanto la cuota base como las actividades opcionales.
CREATE TABLE membresia_actividades (
    id_membresia INT NOT NULL,
    id_actividad INT NOT NULL,
    precio_mensual_congelado DECIMAL(10, 2) NOT NULL,
    CONSTRAINT PK_membresia_actividades PRIMARY KEY CLUSTERED (id_membresia, id_actividad),
    CONSTRAINT FK_membresia_actividades_membresias FOREIGN KEY (id_membresia) REFERENCES membresias(id) ON DELETE CASCADE,
    CONSTRAINT FK_membresia_actividades_actividades FOREIGN KEY (id_actividad) REFERENCES actividades(id) ON DELETE CASCADE
);
GO

-- Tabla: pagos
CREATE TABLE pagos (
    id INT IDENTITY(1,1) NOT NULL,
    id_membresia INT NOT NULL,
    id_metodo_pago INT NOT NULL,
    id_usuario_procesa INT NULL,
    monto DECIMAL(10, 2) NOT NULL,
    fecha_pago DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    CONSTRAINT PK_pagos PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_pagos_membresias FOREIGN KEY (id_membresia) REFERENCES membresias(id),
    CONSTRAINT FK_pagos_metodos_pago FOREIGN KEY (id_metodo_pago) REFERENCES metodos_pago(id),
    CONSTRAINT FK_pagos_usuarios FOREIGN KEY (id_usuario_procesa) REFERENCES usuarios(id) ON DELETE SET NULL,
    CONSTRAINT CHK_pagos_monto CHECK (monto > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_pagos_id_membresia ON pagos(id_membresia);
CREATE NONCLUSTERED INDEX IX_pagos_fecha_pago ON pagos(fecha_pago);
GO

-- Tabla: asistencias
CREATE TABLE asistencias (
    id INT IDENTITY(1,1) NOT NULL,
    id_socio INT NOT NULL,
    fecha_hora_ingreso DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    CONSTRAINT PK_asistencias PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_asistencias_socios FOREIGN KEY (id_socio) REFERENCES socios(id)
);
GO

CREATE NONCLUSTERED INDEX IX_asistencias_id_socio_fecha ON asistencias(id_socio, fecha_hora_ingreso DESC);
GO

-- Tabla: auditoria
CREATE TABLE auditoria (
    id INT IDENTITY(1,1) NOT NULL,
    tabla NVARCHAR(100) NOT NULL,
    operacion NVARCHAR(20) NOT NULL,
    id_usuario INT NULL,
    nombre_usuario NVARCHAR(100) NULL,
    fecha_hora DATETIME NOT NULL DEFAULT GETDATE(),
    valores_anteriores NVARCHAR(MAX) NULL,
    valores_nuevos NVARCHAR(MAX) NULL,
    nombre_entidad NVARCHAR(100) NULL,
    id_entidad NVARCHAR(50) NULL,
    detalles NVARCHAR(500) NULL,
    CONSTRAINT PK_auditoria PRIMARY KEY CLUSTERED (id)
);
GO

CREATE NONCLUSTERED INDEX IX_auditoria_tabla ON auditoria(tabla);
CREATE NONCLUSTERED INDEX IX_auditoria_operacion ON auditoria(operacion);
CREATE NONCLUSTERED INDEX IX_auditoria_id_usuario ON auditoria(id_usuario);
CREATE NONCLUSTERED INDEX IX_auditoria_fecha_hora ON auditoria(fecha_hora);
GO

-- =============================================
-- DATOS INICIALES
-- =============================================

INSERT INTO roles (nombre) VALUES ('superadmin'), ('admin'), ('recepcionista');
GO

-- Usuarios de prueba
INSERT INTO personas (nombre, apellido, email) VALUES
    ('Super', 'Admin', 'superadmin@club.com'),
    ('Admin', 'Club',  'admin@club.com'),
    ('Recepcio', 'Nista', 'recepcionista@club.com');
GO

INSERT INTO usuarios (id_persona, id_rol, nombre_usuario, contrasena_hash) VALUES
    (1, 1, 'superadmin',    'super123'),
    (2, 2, 'admin',         'admin123'),
    (3, 3, 'recepcionista', 'recep123');
GO

INSERT INTO metodos_pago (nombre) VALUES ('Efectivo'), ('Tarjeta'), ('Transferencia');
GO

-- Actividad base obligatoria (siempre incluida en toda membresía)
INSERT INTO actividades (nombre, descripcion, precio_actual, es_cuota_base)
VALUES ('Cuota base', 'Cuota fija mensual obligatoria para todos los socios', 0.00, 1);
GO
