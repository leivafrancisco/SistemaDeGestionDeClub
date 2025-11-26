-- Script para verificar el estado de la base de datos
-- Ejecutar en SQL Server Management Studio

-- Verificar si existe la base de datos
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'gestion_socios')
BEGIN
    PRINT 'La base de datos gestion_socios EXISTE'

    USE gestion_socios;

    -- Verificar tablas
    PRINT ''
    PRINT 'Tablas en la base de datos:'
    SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'

    -- Verificar usuarios
    PRINT ''
    PRINT 'Usuarios en el sistema:'
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'usuarios')
    BEGIN
        SELECT
            u.id,
            u.nombre_usuario,
            u.contrasena_hash,
            u.esta_activo,
            r.nombre as rol,
            p.nombre + ' ' + p.apellido as nombre_completo
        FROM usuarios u
        INNER JOIN personas p ON u.id_persona = p.id
        INNER JOIN roles r ON u.id_rol = r.id
        WHERE u.fecha_eliminacion IS NULL
    END
    ELSE
    BEGIN
        PRINT 'La tabla usuarios NO EXISTE - debe ejecutar init-database.sql'
    END
END
ELSE
BEGIN
    PRINT 'La base de datos gestion_socios NO EXISTE'
    PRINT 'Debe crear la base de datos ejecutando:'
    PRINT 'CREATE DATABASE gestion_socios;'
    PRINT 'Y luego ejecutar el script init-database.sql'
END
