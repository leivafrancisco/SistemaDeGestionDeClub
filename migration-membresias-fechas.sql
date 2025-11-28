-- Script de migración: Cambiar membresías de periodo (mes/año) a fechas
-- Fecha: 2025-11-27
-- Descripción: Elimina las columnas periodo_anio y periodo_mes de la tabla membresias
--              ya que ahora solo se usan fecha_inicio y fecha_fin

USE club_futbol_basico;
GO

-- 1. Primero, verificar que todas las membresías tienen fechas calculadas
SELECT COUNT(*) AS TotalMembresias,
       COUNT(CASE WHEN fecha_inicio IS NOT NULL AND fecha_fin IS NOT NULL THEN 1 END) AS ConFechas
FROM membresias
WHERE fecha_eliminacion IS NULL;
GO

-- 2. Eliminar el índice único que usa periodo_anio y periodo_mes
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_membresias_id_socio_periodo_anio_periodo_mes' AND object_id = OBJECT_ID('membresias'))
BEGIN
    DROP INDEX IX_membresias_id_socio_periodo_anio_periodo_mes ON membresias;
    PRINT 'Índice único eliminado correctamente';
END
ELSE
BEGIN
    PRINT 'El índice no existe o ya fue eliminado';
END
GO

-- 3. Eliminar las columnas periodo_anio y periodo_mes
IF EXISTS (SELECT * FROM sys.columns WHERE name = 'periodo_anio' AND object_id = OBJECT_ID('membresias'))
BEGIN
    ALTER TABLE membresias DROP COLUMN periodo_anio;
    PRINT 'Columna periodo_anio eliminada correctamente';
END
ELSE
BEGIN
    PRINT 'La columna periodo_anio no existe o ya fue eliminada';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE name = 'periodo_mes' AND object_id = OBJECT_ID('membresias'))
BEGIN
    ALTER TABLE membresias DROP COLUMN periodo_mes;
    PRINT 'Columna periodo_mes eliminada correctamente';
END
ELSE
BEGIN
    PRINT 'La columna periodo_mes no existe o ya fue eliminada';
END
GO

-- 4. Verificar la estructura final de la tabla
SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('membresias')
ORDER BY c.column_id;
GO

PRINT 'Migración completada exitosamente';
GO
