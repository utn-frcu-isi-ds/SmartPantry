# SmartPantry

Repositorio integrador de la cátedra Desarrollo de Software 2026.

## Responsables

- Enzo Tanga ([`enzoftv`](https://github.com/enzoftv))
- Esteban Tripodi ([`etripodi`](https://github.com/etripodi))

## Organización

- [`utn-frcu-isi-ds`](https://github.com/utn-frcu-isi-ds)

## Project

- [Desarrollo de Software 2026](https://github.com/orgs/utn-frcu-isi-ds/projects)

## Cómo ejecutar

Requisitos: Visual Studio 2022 o 2026 con el componente Desarrollo de ASP.NET y web, Node.js 24.15 o posterior, Yarn 1.22.x, SQL Server Developer o Express, SSMS, Git y ABP Studio.

1. Abrir `SmartPantry.slnx` con Visual Studio y ejecutar **Build Solution**.
2. Configurar la cadena `ConnectionStrings:Default` en `src/SmartPantry.DbMigrator/appsettings.json` y `src/SmartPantry.HttpApi.Host/appsettings.json`.
3. Ejecutar `SmartPantry.DbMigrator` para crear o actualizar la base de datos.
4. Ejecutar `SmartPantry.HttpApi.Host` y, desde `angular`, ejecutar `yarn start`.

Sin Visual Studio, desde la raíz se puede utilizar `abp install-libs`, `dotnet restore .\SmartPantry.slnx` y `dotnet build .\SmartPantry.slnx --configuration Debug --no-restore`.

## Producto interno

La primera operación vertical permite registrar un producto interno con nombre obligatorio y marca opcional, y recuperarlo por su identificador. La migración `AddProducts` crea la tabla `AppProducts` al ejecutar `SmartPantry.DbMigrator`.

Mientras no se implemente autenticación, los endpoints de `ProductAppService` permiten acceso anónimo sólo para la comprobación académica en Swagger. Esta decisión es temporal y debe revisarse al incorporar seguridad.

Con `SmartPantry.HttpApi.Host` en ejecución, Swagger permite probar la creación mediante `POST /api/app/product` y consultar el resultado mediante `GET /api/app/product/{id}`. Las pruebas se ejecutan con `dotnet test .\SmartPantry.slnx --configuration Release`; las verificaciones de Angular continúan con `yarn build` y `yarn test --watch=false` desde `angular`.

## Estructura de la solución

- `src`: capas Domain, Application, EntityFrameworkCore y HttpApi.Host de la aplicación ABP.
- `test`: proyectos de pruebas de dominio, aplicación e integración.
- `angular`: cliente Angular.
- `etc`: archivos auxiliares generados por ABP.
