# TP09 - Validacion de login y seguridad

Validacion realizada el 9 de octubre de 2026 sobre una base SQL Server de prueba separada de la base de desarrollo.

## Operaciones del repositorio

| Requisito | Servicio o modulo | Endpoint |
| --- | --- | --- |
| RF-05 - Consulta externa | `ExternalProductAppService.GetByBarcodeAsync` | `GET /api/app/external-product/by-barcode` |
| RF-08 - Guardado interno | `ProductAppService.CreateAsync` | `POST /api/app/product` |
| RF-04 - Listado de usuarios | ABP Identity | `GET /api/identity/users` |

Los demas metodos de `ProductAppService` requieren autenticacion. Los servicios de ejemplo `BookAppService` y `AuthorAppService` conservan sus permisos especificos y sus descargas tambien quedan protegidas. El procesamiento del worker permanece interno, sin una API publica ni una sesion de usuario.

## Preparacion y login

- Compilacion y suite completa: 38 pruebas aprobadas, sin casos fallidos ni omitidos.
- Migraciones SQL: Initial, AddProducts y AddPantryExpirationAlerts aplicadas correctamente.
- Seed de Identity y OpenIddict: cuenta administradora y clientes de la plantilla disponibles.
- Swagger por HTTPS: documento OpenAPI responde 200 y ofrece los esquemas `oidc` y `Bearer` como alternativas independientes.
- Login con la pantalla de ABP desde Authorize: probado para administrador y usuario comun.
- Usuario comun creado desde Swagger, activo y sin rol admin.
- Access token copiado del encabezado Authorization del ejemplo Curl y pegado manualmente en Bearer.

## Resultados de Swagger

| Operacion | Condicion | Resultado observado |
| --- | --- | --- |
| RF-05 | Sin sesion de cuenta ni token | 401 |
| RF-05 | Bearer invalido | 401 |
| RF-05 | Bearer valido de usuario comun | 200 y producto real de OpenFoodFacts |
| RF-08 | Sin sesion de cuenta ni token | 401 |
| RF-08 | Usuario comun autenticado mediante oidc | 200 y producto persistido |
| RF-04 | Bearer de usuario comun | 403 |
| RF-04 | Bearer de administrador | 200 |

La consulta externa utilizo el codigo 3017620422003 y devolvio un producto con nombre, marca e imagen. Las pruebas de la logica de integracion continuan utilizando un sustituto del proveedor y no dependen de Internet.

El navegador de validacion comprobo las llamadas sin credencial despues de cerrar la sesion de cuenta de ABP y quitar la autorizacion de Swagger. Para reproducir el ejercicio se recomienda un perfil de prueba separado sin login de cuenta: evita que una cookie autorice una llamada que se pretende probar sin token.

## Pruebas automatizadas

`OperationAuthorizationTests` restaura la autorizacion real de ABP, que la plantilla de tests omite mediante AlwaysAllowAuthorizationService. Comprueba RF-05, todos los metodos CRUD de productos ante un principal anonimo, el uso del catalogo por el usuario comun y el permiso de Identity requerido para RF-04.

`ExpirationAlertProcessorTests` ejecuta el procesamiento con un principal anonimo y comprueba idempotencia, actualizacion y reactivacion de alertas. El host conserva su inicializacion HTTP antes de registrar el worker.

La aplicacion utiliza la autenticacion y la proteccion antiforgery de ABP.

No se incluyen credenciales personales, tokens ni cookies en esta evidencia.
