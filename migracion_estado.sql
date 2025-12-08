-- Script de migración para cambios en membresias y pagos
-- Fecha: 2025-12-05
-- Cambios:
--   1. Agregar columna 'estado' en tabla membresias (AL DIA o VENCIDA)
--   2. Remover columna 'estado_pago' de tabla membresias
--   3. Agregar columna 'estado_pago' en tabla pagos (COMPLETADO, PENDIENTE, ANULADO)

USE gestion_socios;
GO

-- PASO 1: Agregar columna 'estado' en tabla membresias
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'membresias') AND name = 'estado')
BEGIN
    ALTER TABLE membresias
    ADD estado VARCHAR(20) NOT NULL DEFAULT 'AL DIA';

    PRINT 'Columna estado agregada a tabla membresias';
END
ELSE
BEGIN
    PRINT 'La columna estado ya existe en tabla membresias';
END
GO

-- PASO 2: Agregar columna 'estado_pago' en tabla pagos
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'pagos') AND name = 'estado_pago')
BEGIN
    ALTER TABLE pagos
    ADD estado_pago VARCHAR(20) NOT NULL DEFAULT 'COMPLETADO';

    PRINT 'Columna estado_pago agregada a tabla pagos';
END
ELSE
BEGIN
    PRINT 'La columna estado_pago ya existe en tabla pagos';
END
GO

-- PASO 3: Remover columna 'estado_pago' de tabla membresias (solo si existe)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'membresias') AND name = 'estado_pago')
BEGIN
    ALTER TABLE membresias
    DROP COLUMN estado_pago;

    PRINT 'Columna estado_pago removida de tabla membresias';
END
ELSE
BEGIN
    PRINT 'La columna estado_pago no existe en tabla membresias (ya fue removida)';
END
GO

-- PASO 4: Actualizar registros existentes de pagos con estado COMPLETADO
UPDATE pagos
SET estado_pago = 'COMPLETADO'
WHERE fecha_eliminacion IS NULL;

PRINT 'Migración completada exitosamente';
GO
