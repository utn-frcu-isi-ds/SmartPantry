# Flujo de contribución

## Ramas

- `main`: integración principal y estable.
- `stg`: versión candidata a prueba.
- `prod`: versión aprobada para entrega o defensa.
- `feature/configuracion-inicial`: rama especial utilizada exclusivamente en el TP02.
- `feature/<numero-issue>-<descripcion-corta>`: convención para trabajos posteriores que tengan un Issue asociado.

## Flujo del TP02

1. Actualizar `main` y crear desde allí `feature/configuracion-inicial`.
2. Realizar commits pequeños y descriptivos.
3. Abrir el Pull Request contra `main` y completar su descripción.
4. Solicitar la revisión de `@utn-frcu-isi-ds/reviewers`.
5. No integrar el Pull Request hasta que tenga la aprobación requerida.
6. No hacer push directo a `main`, `stg` ni `prod`.

En el TP02 no es obligatorio crear un Issue ni incluir `Closes #N`.

## Flujo para trabajos posteriores

Cuando la consigna correspondiente requiera un Issue:

1. Crear el Issue y, si corresponde, agregarlo al Project de la organización.
2. Crear una rama desde la rama base indicada por el práctico.
3. Vincular commits y Pull Request con el Issue.
4. Integrar únicamente después de obtener la aprobación requerida.

Tras integrar capacidades aprobadas, el flujo previsto es `main` → `stg` y luego `stg` → `prod`, cuando la consigna lo indique.
