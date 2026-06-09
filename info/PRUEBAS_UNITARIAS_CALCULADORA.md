# Pruebas Unitarias — Clase Calculadora

**Carrera:** Lic. en Sistemas de Información
**Asignatura:** Ingeniería del Software II — FACENA UNNE

---

## Método bajo prueba

```java
public int multiplicar(i, j) { ... }
```

Acepta dos valores enteros como parámetro y retorna un valor entero de resultado.
La calculadora trabaja **solamente con números enteros**.

---

## Análisis de tipos de entrada

Antes de definir las pruebas, se analizan los tipos de entrada posibles para `i` y `j`:

| Tipo | Ejemplos |
|------|----------|
| Entero positivo | 1, 2, 100 |
| Entero negativo | -1, -3, -10 |
| Cero | 0 |
| Carácter / no numérico | "d", "a", "#" |
| Null | null |
| Número muy grande (desbordamiento) | 999999999 |

---

## Tabla de Pruebas Unitarias

| i | j | Descripción | Resultado esperado |
|---|---|-------------|-------------------|
| 2 | 2 | Multiplicación normal de dos enteros positivos | 4 |
| d | 5 | Multiplicación entre un carácter y un número positivo | Error |
| -1 | 4 | Multiplicación normal entre un número negativo y otro positivo | -4 |
| 0 | 7 | Multiplicación de cero por un entero positivo | 0 |
| -3 | -2 | Multiplicación de dos números negativos | 6 |
| 0 | 0 | Multiplicación de cero por cero | 0 |
| null | 5 | Multiplicación con un valor nulo | Error |
| 999999999 | 999999999 | Multiplicación con números muy grandes (desbordamiento) | Error |

---

## Justificación de cada caso

**Caso 1 — Dos positivos (2 × 2 = 4)**
Caso base y más frecuente de uso. Verifica el comportamiento normal del método.

**Caso 2 — Carácter y positivo (d × 5)**
El método solo acepta enteros. Un carácter no es un tipo válido, debe lanzar un error.

**Caso 3 — Negativo y positivo (-1 × 4 = -4)**
Verifica que el método maneje correctamente el signo del resultado.

**Caso 4 — Cero y positivo (0 × 7 = 0)**
Cualquier número multiplicado por cero debe dar cero. Caso límite importante.

**Caso 5 — Dos negativos (-3 × -2 = 6)**
Negativo por negativo da positivo. Verifica el manejo correcto del doble signo negativo.

**Caso 6 — Cero por cero (0 × 0 = 0)**
Caso extremo del valor límite cero. El resultado sigue siendo cero.

**Caso 7 — Valor nulo (null × 5)**
Un valor nulo no es un entero válido. El método debe manejar este caso con un error.

**Caso 8 — Números muy grandes (desbordamiento)**
Al multiplicar enteros muy grandes el resultado puede superar el límite del tipo `int`, produciendo un desbordamiento (overflow). Debe retornar error o manejarse explícitamente.
