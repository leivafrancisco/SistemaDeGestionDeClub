-- =============================================================
-- Migración: Agregar tabla cuotas
-- Descripción: Agrega la tabla de cuotas mensuales entre
--              membresias y pagos para gestión de morosos.
-- =============================================================

-- 1. Crear tabla cuotas
CREATE TABLE IF NOT EXISTS cuotas (
    id                  SERIAL PRIMARY KEY,
    id_membresia        INTEGER NOT NULL,
    numero_cuota        INTEGER NOT NULL,
    monto               DECIMAL(10, 2) NOT NULL CHECK (monto > 0),
    fecha_vencimiento   DATE NOT NULL,
    estado              VARCHAR(20) NOT NULL DEFAULT 'pendiente'
                            CHECK (estado IN ('pendiente', 'pagada', 'vencida')),
    fecha_creacion      TIMESTAMP NOT NULL DEFAULT NOW(),
    fecha_actualizacion TIMESTAMP NOT NULL DEFAULT NOW(),
    fecha_eliminacion   TIMESTAMP,

    CONSTRAINT fk_cuotas_membresia
        FOREIGN KEY (id_membresia) REFERENCES membresias(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_cuotas_membresia_numero
        UNIQUE (id_membresia, numero_cuota)
);

-- 2. Índices para consultas frecuentes
CREATE INDEX IF NOT EXISTS idx_cuotas_membresia    ON cuotas (id_membresia);
CREATE INDEX IF NOT EXISTS idx_cuotas_vencimiento  ON cuotas (fecha_vencimiento);
CREATE INDEX IF NOT EXISTS idx_cuotas_estado       ON cuotas (estado);

-- 3. Agregar columna id_cuota a pagos (nullable, FK a cuotas)
ALTER TABLE pagos
    ADD COLUMN IF NOT EXISTS id_cuota INTEGER
        REFERENCES cuotas(id) ON DELETE SET NULL;

CREATE INDEX IF NOT EXISTS idx_pagos_cuota ON pagos (id_cuota);

-- =============================================================
-- Generar cuotas para membresías existentes (opcional)
-- Ejecutar solo si se quiere retroactivamente generar cuotas.
-- Ajustar la lógica según necesidad.
-- =============================================================
-- INSERT INTO cuotas (id_membresia, numero_cuota, monto, fecha_vencimiento, estado)
-- SELECT
--     m.id,
--     gs.numero_mes,
--     ROUND(m.costo_total::numeric /
--           (DATE_PART('year', m.fecha_fin) - DATE_PART('year', m.fecha_inicio)) * 12 +
--           (DATE_PART('month', m.fecha_fin) - DATE_PART('month', m.fecha_inicio)) + 1, 2),
--     (DATE_TRUNC('month', m.fecha_inicio) + (gs.numero_mes - 1) * INTERVAL '1 month'
--         + INTERVAL '1 month' - INTERVAL '1 day')::date,
--     CASE
--         WHEN (DATE_TRUNC('month', m.fecha_inicio) + (gs.numero_mes - 1) * INTERVAL '1 month'
--               + INTERVAL '1 month' - INTERVAL '1 day')::date < CURRENT_DATE
--         THEN 'vencida'
--         ELSE 'pendiente'
--     END
-- FROM membresias m
-- CROSS JOIN generate_series(1,
--     (DATE_PART('year', m.fecha_fin) - DATE_PART('year', m.fecha_inicio))::int * 12 +
--     (DATE_PART('month', m.fecha_fin) - DATE_PART('month', m.fecha_inicio))::int + 1
-- ) AS gs(numero_mes)
-- WHERE m.fecha_eliminacion IS NULL
--   AND NOT EXISTS (SELECT 1 FROM cuotas c WHERE c.id_membresia = m.id);
