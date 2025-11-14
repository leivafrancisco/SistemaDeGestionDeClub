-- Script de inicialización de la base de datos
-- Sistema de Gestión de Club de Fútbol

USE club_futbol_basico;
GO

-- =============================================
-- CREAR TABLAS
-- =============================================

-- Tabla roles
CREATE TABLE roles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

-- Tabla personas
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

-- Tabla usuarios
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

-- Tabla socios
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

-- Tabla metodos_pago
CREATE TABLE metodos_pago (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

-- Tabla actividades
CREATE TABLE actividades (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(500),
    precio DECIMAL(10, 2) NOT NULL,
    es_cuota_base BIT NOT NULL DEFAULT 0,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL
);

-- Tabla membresias
CREATE TABLE membresias (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_socio INT NOT NULL,
    periodo_anio SMALLINT NOT NULL,
    periodo_mes TINYINT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    FOREIGN KEY (id_socio) REFERENCES socios(id),
    UNIQUE (id_socio, periodo_anio, periodo_mes)
);

-- Tabla membresia_actividades
CREATE TABLE membresia_actividades (
    id_membresia INT NOT NULL,
    id_actividad INT NOT NULL,
    precio_al_momento DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (id_membresia, id_actividad),
    FOREIGN KEY (id_membresia) REFERENCES membresias(id),
    FOREIGN KEY (id_actividad) REFERENCES actividades(id)
);

-- Tabla pagos
CREATE TABLE pagos (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_membresia INT NOT NULL,
    id_metodo_pago INT NOT NULL,
    id_usuario_procesa INT NULL,
    monto DECIMAL(10, 2) NOT NULL,
    fecha_pago DATE NOT NULL,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    fecha_actualizacion DATETIME DEFAULT GETDATE(),
    fecha_eliminacion DATETIME NULL,
    FOREIGN KEY (id_membresia) REFERENCES membresias(id),
    FOREIGN KEY (id_metodo_pago) REFERENCES metodos_pago(id),
    FOREIGN KEY (id_usuario_procesa) REFERENCES usuarios(id)
);

-- Tabla asistencias
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
-- CREAR ÍNDICES
-- =============================================

CREATE INDEX IX_Personas_Email ON personas(email);
CREATE INDEX IX_Personas_DNI ON personas(dni);
CREATE INDEX IX_Socios_NumeroSocio ON socios(numero_socio);
CREATE INDEX IX_Socios_Persona ON socios(id_persona);
CREATE INDEX IX_Membresias_SocioPeriodo ON membresias(id_socio, periodo_anio, periodo_mes);
CREATE INDEX IX_Pagos_Membresia ON pagos(id_membresia);
CREATE INDEX IX_Pagos_Fecha ON pagos(fecha_pago);
CREATE INDEX IX_Asistencias_SocioFecha ON asistencias(id_socio, fecha_hora_ingreso DESC);

-- =============================================
-- INSERTAR DATOS INICIALES
-- =============================================

-- Roles
INSERT INTO roles (nombre) VALUES 
('superadmin'),
('admin'),
('recepcionista');

-- Métodos de pago
INSERT INTO metodos_pago (nombre) VALUES 
('Efectivo'),
('Tarjeta'),
('Transferencia');

-- Actividades
INSERT INTO actividades (nombre, descripcion, precio, es_cuota_base) VALUES 
('Cuota Social', 'Cuota mensual básica del club', 5000.00, 1),
('Fútbol 5', 'Cancha de fútbol 5', 3000.00, 0),
('Fútbol 11', 'Cancha de fútbol 11', 5000.00, 0),
('Gimnasio', 'Acceso al gimnasio del club', 4000.00, 0),
('Natación', 'Clases de natación', 3500.00, 0),
('Paddle', 'Cancha de paddle', 2500.00, 0);

-- Personas para usuarios del sistema
INSERT INTO personas (nombre, apellido, email, dni, fecha_nacimiento) VALUES 
('Admin', 'Sistema', 'admin@club.com', '11111111', '1980-01-01'),
('Recepcionista', 'Principal', 'recepcionista@club.com', '22222222', '1990-05-15');

-- Usuarios del sistema (Contraseñas en texto plano para DEV - en producción usar BCrypt)
INSERT INTO usuarios (id_persona, id_rol, nombre_usuario, contrasena_hash, esta_activo) VALUES 
(1, 2, 'admin', 'admin123', 1),          -- Admin
(2, 3, 'recepcionista', 'recep123', 1);  -- Recepcionista

-- Personas para socios de prueba
INSERT INTO personas (nombre, apellido, email, dni, fecha_nacimiento) VALUES 
('Juan', 'Pérez', 'juan.perez@email.com', '30111222', '1985-03-15'),
('María', 'González', 'maria.gonzalez@email.com', '32555666', '1992-07-20'),
('Carlos', 'Rodríguez', 'carlos.rodriguez@email.com', '28999888', '1988-11-10'),
('Ana', 'Martínez', 'ana.martinez@email.com', '35222111', '1995-02-28'),
('Luis', 'Fernández', 'luis.fernandez@email.com', '27444555', '1983-09-05'),
('Laura', 'García', 'laura.garcia@email.com', '33777888', '1990-12-18'),
('Pedro', 'López', 'pedro.lopez@email.com', '29333444', '1987-04-22'),
('Sofía', 'Sánchez', 'sofia.sanchez@email.com', '34888999', '1993-08-14'),
('Diego', 'Ramírez', 'diego.ramirez@email.com', '31555666', '1991-06-30'),
('Valentina', 'Torres', 'valentina.torres@email.com', '36111222', '1996-01-25');

-- Socios (asociar personas 3-12 como socios)
INSERT INTO socios (id_persona, numero_socio, esta_activo, fecha_alta) VALUES 
(3, 'SOC-001', 1, '2023-01-15'),
(4, 'SOC-002', 1, '2023-02-20'),
(5, 'SOC-003', 1, '2023-03-10'),
(6, 'SOC-004', 1, '2023-04-05'),
(7, 'SOC-005', 1, '2023-05-12'),
(8, 'SOC-006', 1, '2023-06-18'),
(9, 'SOC-007', 1, '2023-07-22'),
(10, 'SOC-008', 1, '2023-08-14'),
(11, 'SOC-009', 0, '2023-09-30'),  -- Socio inactivo
(12, 'SOC-010', 1, '2023-10-25');

-- Membresías de ejemplo (Noviembre 2025)
INSERT INTO membresias (id_socio, periodo_anio, periodo_mes, fecha_inicio, fecha_fin) VALUES 
(1, 2025, 11, '2025-11-01', '2025-11-30'),
(2, 2025, 11, '2025-11-01', '2025-11-30'),
(3, 2025, 11, '2025-11-01', '2025-11-30'),
(4, 2025, 11, '2025-11-01', '2025-11-30'),
(5, 2025, 11, '2025-11-01', '2025-11-30');

-- Actividades en membresías (cuota social + actividades)
-- Socio 1: Cuota + Fútbol 5
INSERT INTO membresia_actividades (id_membresia, id_actividad, precio_al_momento) VALUES 
(1, 1, 5000.00),  -- Cuota Social
(1, 2, 3000.00);  -- Fútbol 5

-- Socio 2: Cuota + Gimnasio
INSERT INTO membresia_actividades (id_membresia, id_actividad, precio_al_momento) VALUES 
(2, 1, 5000.00),  -- Cuota Social
(2, 4, 4000.00);  -- Gimnasio

-- Socio 3: Cuota + Fútbol 11 + Paddle
INSERT INTO membresia_actividades (id_membresia, id_actividad, precio_al_momento) VALUES 
(3, 1, 5000.00),  -- Cuota Social
(3, 3, 5000.00),  -- Fútbol 11
(3, 6, 2500.00);  -- Paddle

-- Socio 4: Solo cuota
INSERT INTO membresia_actividades (id_membresia, id_actividad, precio_al_momento) VALUES 
(4, 1, 5000.00);  -- Cuota Social

-- Socio 5: Cuota + Natación
INSERT INTO membresia_actividades (id_membresia, id_actividad, precio_al_momento) VALUES 
(5, 1, 5000.00),  -- Cuota Social
(5, 5, 3500.00);  -- Natación

-- Pagos de ejemplo
INSERT INTO pagos (id_membresia, id_metodo_pago, id_usuario_procesa, monto, fecha_pago) VALUES 
(1, 1, 1, 8000.00, '2025-11-05'),  -- Pago completo socio 1
(2, 2, 1, 9000.00, '2025-11-03'),  -- Pago completo socio 2
(3, 3, 1, 6000.00, '2025-11-07'),  -- Pago parcial socio 3
(4, 1, 2, 5000.00, '2025-11-02');  -- Pago completo socio 4

-- Asistencias de ejemplo (últimos 7 días)
INSERT INTO asistencias (id_socio, fecha_hora_ingreso) VALUES 
(1, '2025-11-14 18:30:00'),
(2, '2025-11-14 19:00:00'),
(3, '2025-11-14 17:45:00'),
(1, '2025-11-13 18:15:00'),
(4, '2025-11-13 19:30:00'),
(2, '2025-11-12 18:45:00'),
(5, '2025-11-12 17:00:00'),
(1, '2025-11-11 18:30:00'),
(3, '2025-11-11 19:15:00'),
(6, '2025-11-10 18:00:00');

GO

-- =============================================
-- VERIFICAR DATOS
-- =============================================

PRINT 'Base de datos inicializada correctamente';
PRINT '';
PRINT 'Resumen de datos insertados:';
SELECT 'Roles' AS Tabla, COUNT(*) AS Cantidad FROM roles
UNION ALL
SELECT 'Personas', COUNT(*) FROM personas
UNION ALL
SELECT 'Usuarios', COUNT(*) FROM usuarios
UNION ALL
SELECT 'Socios', COUNT(*) FROM socios
UNION ALL
SELECT 'Actividades', COUNT(*) FROM actividades
UNION ALL
SELECT 'Métodos de Pago', COUNT(*) FROM metodos_pago
UNION ALL
SELECT 'Membresías', COUNT(*) FROM membresias
UNION ALL
SELECT 'Pagos', COUNT(*) FROM pagos
UNION ALL
SELECT 'Asistencias', COUNT(*) FROM asistencias;

GO
