USE gestion_club;
GO

-- =============================================
-- STORED PROCEDURE 1 (CONSULTA)
-- sp_ResumenSocio
--
-- Devuelve un resumen financiero completo de un socio:
-- total de membresías, monto cargado, monto pagado,
-- saldo pendiente, fecha del último pago y asistencias
-- del mes en curso.
--
-- Invocación de ejemplo:
--   EXEC sp_ResumenSocio @id_socio = 1;
-- =============================================
CREATE OR ALTER PROCEDURE sp_ResumenSocio
    @id_socio INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar que el socio exista
    IF NOT EXISTS (
        SELECT 1 FROM socios WHERE id = @id_socio AND fecha_eliminacion IS NULL
    )
    BEGIN
        RAISERROR('El socio con id %d no existe o fue eliminado.', 16, 1, @id_socio);
        RETURN;
    END

    SELECT
        s.id                                                    AS id_socio,
        p.nombre + ' ' + p.apellido                            AS nombre_completo,
        s.numero_socio,
        p.email,
        p.dni,
        s.esta_activo,
        s.fecha_alta,
        s.fecha_baja,

        -- Totales de membresías
        COUNT(DISTINCT m.id)                                    AS total_membresias,
        ISNULL(SUM(DISTINCT m.costo_total), 0)                 AS total_cargado,

        -- Total efectivamente pagado (sum de todos los pagos sobre cuotas de este socio)
        ISNULL((
            SELECT SUM(pg.monto)
            FROM pagos    pg
            JOIN cuotas   c2 ON c2.id          = pg.id_cuota
            JOIN membresias m2 ON m2.id        = c2.id_membresia
            WHERE m2.id_socio                  = s.id
              AND pg.fecha_eliminacion         IS NULL
              AND m2.fecha_eliminacion         IS NULL
        ), 0)                                                   AS total_pagado,

        -- Saldo pendiente
        ISNULL(SUM(DISTINCT m.costo_total), 0) - ISNULL((
            SELECT SUM(pg.monto)
            FROM pagos    pg
            JOIN cuotas   c2 ON c2.id          = pg.id_cuota
            JOIN membresias m2 ON m2.id        = c2.id_membresia
            WHERE m2.id_socio                  = s.id
              AND pg.fecha_eliminacion         IS NULL
              AND m2.fecha_eliminacion         IS NULL
        ), 0)                                                   AS saldo_pendiente,

        -- Fecha del último pago registrado
        (
            SELECT MAX(pg.fecha_pago)
            FROM pagos    pg
            JOIN cuotas   c2 ON c2.id          = pg.id_cuota
            JOIN membresias m2 ON m2.id        = c2.id_membresia
            WHERE m2.id_socio                  = s.id
              AND pg.fecha_eliminacion         IS NULL
        )                                                       AS ultima_fecha_pago,

        -- Asistencias en el mes calendario actual
        (
            SELECT COUNT(*)
            FROM asistencias a
            WHERE a.id_socio                   = s.id
              AND MONTH(a.fecha_hora_ingreso)  = MONTH(GETDATE())
              AND YEAR(a.fecha_hora_ingreso)   = YEAR(GETDATE())
              AND a.fecha_eliminacion          IS NULL
        )                                                       AS asistencias_este_mes

    FROM socios       s
    JOIN personas     p  ON p.id  = s.id_persona
    LEFT JOIN membresias m ON m.id_socio = s.id AND m.fecha_eliminacion IS NULL
    WHERE s.id = @id_socio
    GROUP BY
        s.id, p.nombre, p.apellido, s.numero_socio,
        p.email, p.dni, s.esta_activo, s.fecha_alta, s.fecha_baja;
END
GO


-- =============================================
-- STORED PROCEDURE 2 (ACTUALIZACIÓN)
-- sp_CambiarEstadoSocio
--
-- Activa o desactiva un socio. Al desactivarlo registra
-- la fecha_baja; al reactivarlo la limpia.
-- Además inserta un registro en la tabla auditoria.
--
-- Invocación de ejemplo:
--   EXEC sp_CambiarEstadoSocio @id_socio = 1, @esta_activo = 0, @id_usuario_procesa = 2;
--   EXEC sp_CambiarEstadoSocio @id_socio = 1, @esta_activo = 1, @id_usuario_procesa = 2;
-- =============================================
CREATE OR ALTER PROCEDURE sp_CambiarEstadoSocio
    @id_socio           INT,
    @esta_activo        BIT,
    @id_usuario_procesa INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar que el socio exista
    IF NOT EXISTS (
        SELECT 1 FROM socios WHERE id = @id_socio AND fecha_eliminacion IS NULL
    )
    BEGIN
        RAISERROR('El socio con id %d no existe o fue eliminado.', 16, 1, @id_socio);
        RETURN;
    END

    -- Nombre del socio para el registro de auditoría
    DECLARE @nombre_socio NVARCHAR(200);
    SELECT @nombre_socio = p.nombre + ' ' + p.apellido
    FROM socios s
    JOIN personas p ON p.id = s.id_persona
    WHERE s.id = @id_socio;

    -- Estado anterior para auditoría
    DECLARE @estado_anterior BIT;
    SELECT @estado_anterior = esta_activo FROM socios WHERE id = @id_socio;

    -- Actualizar el socio
    UPDATE socios
    SET
        esta_activo         = @esta_activo,
        fecha_baja          = CASE
                                WHEN @esta_activo = 0 THEN CAST(GETDATE() AS DATE)
                                ELSE NULL
                              END,
        fecha_actualizacion = GETDATE()
    WHERE id = @id_socio;

    -- Registrar en auditoría
    INSERT INTO auditoria (
        tabla, operacion, id_usuario, fecha_hora,
        nombre_entidad, id_entidad, detalles,
        valores_anteriores, valores_nuevos
    )
    VALUES (
        'socios',
        'UPDATE',
        @id_usuario_procesa,
        GETDATE(),
        @nombre_socio,
        CAST(@id_socio AS VARCHAR(50)),
        CASE
            WHEN @esta_activo = 1 THEN 'Socio reactivado'
            ELSE 'Socio dado de baja'
        END,
        '{"esta_activo": ' + CAST(@estado_anterior AS VARCHAR) + '}',
        '{"esta_activo": ' + CAST(@esta_activo    AS VARCHAR) + '}'
    );

    -- Devolver el socio actualizado
    SELECT
        s.id,
        s.numero_socio,
        s.esta_activo,
        s.fecha_alta,
        s.fecha_baja,
        p.nombre,
        p.apellido,
        p.email,
        p.dni
    FROM socios s
    JOIN personas p ON p.id = s.id_persona
    WHERE s.id = @id_socio;
END
GO
